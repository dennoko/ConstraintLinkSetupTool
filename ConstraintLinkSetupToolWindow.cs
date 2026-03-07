using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Tiloop.ConstraintLinkSetupTool.Core.Models;
using Tiloop.ConstraintLinkSetupTool.Core.Services;

namespace Tiloop.ConstraintLinkSetupTool.UI
{
    public class ConstraintLinkSetupToolWindow : EditorWindow
    {
        private SetupConfig _config = new SetupConfig();
        private List<BonePair> _bonePairs = new List<BonePair>();

        private BoneMappingService _mappingService;
        private ConstraintSetupService _setupService;

        private Vector2 _outerScrollPos;
        private Vector2 _mappingScrollPos;
        private bool _showAdvancedSettings = false;
        private bool _debugMode = false;
        private bool _constraintsApplied = false;

        // UI Styles
        private GUIStyle _headerStyle;
        private GUIStyle _stepBoxStyle;
        private GUIStyle _stepTitleStyle;

        // Localization
        private const string LangRelativePath = "Editor/ConstraintLinkSetupTool/Docs/Lang";
        private bool _isEnglish = false;
        private LocalizationData _texts;

        [MenuItem("dennokoworks/Constraint Link Setup Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConstraintLinkSetupToolWindow>("Constraint Link Setup Tool");
            window.minSize = new Vector2(420, 500);
            window.Show();
        }

        private void OnEnable()
        {
            _mappingService = new BoneMappingService();
            _setupService = new ConstraintSetupService();

            _isEnglish = EditorPrefs.GetBool("ConstraintLink_Lang_EN", false);
            LoadLanguage();
        }

        private void LoadLanguage()
        {
            string fileName = _isEnglish ? "en.json" : "ja.json";
            string path = Path.Combine(Application.dataPath, LangRelativePath, fileName);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                _texts = JsonUtility.FromJson<LocalizationData>(json);
            }
            else
            {
                _texts = new LocalizationData();
                Debug.LogWarning("Language file not found: " + path);
            }
            
            this.titleContent = new GUIContent(_texts.WindowTitle ?? "Constraint Link");
        }

        private void InitializeStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                margin = new RectOffset(0, 0, 8, 4)
            };

            _stepBoxStyle = new GUIStyle("HelpBox")
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 0, 8)
            };

            _stepTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                margin = new RectOffset(0, 0, 0, 4)
            };
        }

        private static void DrawSeparator()
        {
            EditorGUILayout.Space(3);
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            EditorGUILayout.Space(3);
        }

        private void OnGUI()
        {
            if (_texts == null) LoadLanguage();
            InitializeStyles();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(_texts.WindowTitle ?? "Constraint Link Setup Tool", _headerStyle);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            _isEnglish = EditorGUILayout.ToggleLeft("English", _isEnglish, GUILayout.Width(65));
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("ConstraintLink_Lang_EN", _isEnglish);
                LoadLanguage();
            }
            EditorGUILayout.EndHorizontal();

            DrawSeparator();

            _outerScrollPos = EditorGUILayout.BeginScrollView(_outerScrollPos);

            EditorGUI.BeginChangeCheck();
            DrawStep1BaseBoneSetup();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateAutoMapping();
            }

            DrawStep2AutoMapping();
            DrawStep3Execute();

            EditorGUILayout.EndScrollView();
        }

        private void UpdateAutoMapping()
        {
            if (_config.TargetProstheticBaseBone != null)
            {
                _config.SetAutoDetectedAvatarBaseBone(
                    FindMatchingAvatarBone(_config.TargetProstheticBaseBone));
            }
            else
            {
                _config.SetAutoDetectedAvatarBaseBone(null);
            }

            if (_config.TargetAvatarBaseBone != null && _config.TargetProstheticBaseBone != null)
            {
                _mappingService.DebugMode = _debugMode;
                _bonePairs = _mappingService.MatchBones(
                    _config.TargetAvatarRoot, _config.TargetProstheticRoot,
                    _config.TargetAvatarBaseBone, _config.TargetProstheticBaseBone,
                    _config.PartSideMode);
                _constraintsApplied = false;
            }
            else
            {
                if (_bonePairs != null) _bonePairs.Clear();
            }
        }

        /// <summary>
        /// 義手側ベースボーンと同名のボーンをアバター階層から検索する
        /// 義手サブツリーは検索対象から除外する
        /// </summary>
        private Transform FindMatchingAvatarBone(Transform prostheticBaseBone)
        {
            if (prostheticBaseBone == null || prostheticBaseBone.parent == null)
                return null;

            Transform attachmentParent = prostheticBaseBone.parent;
            Transform avatarRoot = attachmentParent.root;
            string normalizedTarget = BoneNameMatcher.NormalizeBoneName(prostheticBaseBone.name);

            // prostheticBaseBoneからattachmentParentの直接の子まで上に辿る（除外サブツリー特定）
            Transform prostheticSubtreeRoot = prostheticBaseBone;
            while (prostheticSubtreeRoot.parent != attachmentParent)
            {
                if (prostheticSubtreeRoot.parent == null)
                    return attachmentParent; // 想定外の構造: 親にフォールバック
                prostheticSubtreeRoot = prostheticSubtreeRoot.parent;
            }

            // アバター階層全体を検索（義手サブツリーを除外）
            Transform found = SearchHierarchyForBone(avatarRoot, prostheticSubtreeRoot, normalizedTarget);
            return found ?? attachmentParent; // 見つからない場合は元の挙動（親）にフォールバック
        }

        private static Transform SearchHierarchyForBone(Transform current, Transform excludeSubtree, string normalizedName)
        {
            if (current == excludeSubtree) return null;
            if (BoneNameMatcher.NormalizeBoneName(current.name) == normalizedName) return current;
            foreach (Transform child in current)
            {
                var result = SearchHierarchyForBone(child, excludeSubtree, normalizedName);
                if (result != null) return result;
            }
            return null;
        }

        private void DrawStep1BaseBoneSetup()
        {
            EditorGUILayout.BeginVertical(_stepBoxStyle);
            GUILayout.Label(_texts.Step1Title ?? "Step 1: Base Bone Setup", _stepTitleStyle);
            EditorGUILayout.HelpBox(_texts.Step1Desc, MessageType.Info);

            EditorGUILayout.Space(6);

            // Prosthetic base bone
            EditorGUILayout.LabelField(_texts.ProstheticBaseBone ?? "Prosthetic Base Bone", EditorStyles.boldLabel);
            _config.TargetProstheticBaseBone = (Transform)EditorGUILayout.ObjectField(
                _config.TargetProstheticBaseBone, typeof(Transform), true);

            EditorGUILayout.Space(8);

            // Avatar base bone
            EditorGUILayout.LabelField(_texts.AvatarBaseBoneOptional ?? "Avatar Base Bone (Optional)", EditorStyles.boldLabel);

            // Resolved bone (always visible, read-only)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(
                _texts.AutoDetectedAvatarBaseBone ?? "Resolved",
                _config.TargetAvatarBaseBone, typeof(Transform), true);
            EditorGUI.EndDisabledGroup();

            if (_config.TargetProstheticBaseBone != null && _config.TargetAvatarBaseBone == null)
            {
                EditorGUILayout.HelpBox(
                    _texts.ErrorAutoResolveAvatarBaseBone ?? _texts.ErrorNoBaseBone,
                    MessageType.Warning);
            }

            // Manual override toggle + field
            _config.UseManualAvatarBaseBone = EditorGUILayout.ToggleLeft(
                _texts.UseManualAvatarBaseBone ?? "Use Manual Avatar Base Bone",
                _config.UseManualAvatarBaseBone);

            if (_config.UseManualAvatarBaseBone)
            {
                EditorGUI.indentLevel++;
                _config.ManualAvatarBaseBone = (Transform)EditorGUILayout.ObjectField(
                    _config.ManualAvatarBaseBone, typeof(Transform), true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);

            // Advanced Settings Foldout
            _showAdvancedSettings = EditorGUILayout.Foldout(
                _showAdvancedSettings, _texts.AdvancedFoldout ?? "Advanced Settings", true, EditorStyles.foldoutHeader);
            if (_showAdvancedSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.Space(4);

                string[] sideOptions =
                {
                    _texts.SideAuto ?? "Auto",
                    _texts.SideRight ?? "Right",
                    _texts.SideLeft ?? "Left"
                };
                _config.PartSideMode = (SideMode)EditorGUILayout.Popup(
                    _texts.SideMode ?? "Side Mode",
                    (int)_config.PartSideMode,
                    sideOptions);

                EditorGUILayout.Space(3);

                _debugMode = EditorGUILayout.ToggleLeft(
                    "Debug Mode (Assets/Editor/ConstraintLinkSetupTool/Log/)",
                    _debugMode, EditorStyles.miniLabel);

                EditorGUILayout.Space(4);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStep2AutoMapping()
        {
            int pairCount = _bonePairs != null ? _bonePairs.Count : 0;
            string step2Header = _texts.Step2Title ?? "Step 2: Mapping Preview";
            if (pairCount > 0)
                step2Header += $"  ({pairCount})";

            EditorGUILayout.BeginVertical(_stepBoxStyle);
            GUILayout.Label(step2Header, _stepTitleStyle);

            // Table Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(_texts.ColUse ?? "Use", EditorStyles.miniLabel, GUILayout.Width(28));
            GUILayout.Label(_texts.ColAvatarBone ?? "Avatar Bone", EditorStyles.miniLabel);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.Label(_texts.ColProstheticBone ?? "Prosthetic Bone", EditorStyles.miniLabel);
            GUILayout.Label("", GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            var lineRect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.25f));

            // Scroll view with height adaptive to content
            float scrollHeight = Mathf.Clamp(pairCount * 21f + 10f, 80f, 260f);
            _mappingScrollPos = EditorGUILayout.BeginScrollView(
                _mappingScrollPos, "box", GUILayout.Height(scrollHeight));

            if (pairCount > 0)
            {
                for (int i = 0; i < _bonePairs.Count; i++)
                {
                    var pair = _bonePairs[i];
                    EditorGUILayout.BeginHorizontal();

                    pair.ApplyConstraint = EditorGUILayout.Toggle(pair.ApplyConstraint, GUILayout.Width(20));
                    pair.AvatarBone = (Transform)EditorGUILayout.ObjectField(pair.AvatarBone, typeof(Transform), true);
                    GUILayout.Label("↔", GUILayout.Width(20));
                    pair.ProstheticBone = (Transform)EditorGUILayout.ObjectField(pair.ProstheticBone, typeof(Transform), true);

                    if (pair.IsBaseBone)
                        GUILayout.Label(_texts.LabelBase ?? "(Base)", EditorStyles.miniBoldLabel, GUILayout.Width(40));
                    else
                        GUILayout.Space(44);

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox(_texts.NoMappingMsg ?? "No mapping generated.", MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawStep3Execute()
        {
            EditorGUILayout.BeginVertical(_stepBoxStyle);
            GUILayout.Label(_texts.Step3Title ?? "Step 3: Execute", _stepTitleStyle);

            EditorGUILayout.Space(4);

            Color oldBg = GUI.backgroundColor;
            bool canExecute = _config.IsValid() && _bonePairs != null && _bonePairs.Count > 0;

            // Applied status indicator
            if (_constraintsApplied)
            {
                EditorGUILayout.HelpBox(_texts.CompleteMsg ?? "Constraints applied.", MessageType.None);
            }

            EditorGUI.BeginDisabledGroup(!canExecute);
            GUI.backgroundColor = canExecute ? new Color(0.3f, 0.8f, 0.4f) : oldBg;
            if (GUILayout.Button(_texts.ExecuteBtn ?? "Execute", GUILayout.Height(38)))
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Constraint Setup");
                int undoGroup = Undo.GetCurrentGroup();

                _setupService.ApplyConstraints(_bonePairs, _config);
                _constraintsApplied = true;

                Undo.CollapseUndoOperations(undoGroup);
                EditorUtility.DisplayDialog("Complete", _texts.CompleteMsg, "OK");
            }
            GUI.backgroundColor = oldBg;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(6);
            DrawSeparator();

            EditorGUI.BeginDisabledGroup(!_constraintsApplied);
            GUI.backgroundColor = _constraintsApplied ? new Color(0.9f, 0.4f, 0.4f) : oldBg;
            if (GUILayout.Button(_texts.RevertBtn ?? "Remove Constraints (Revert)", GUILayout.Height(24)))
            {
                if (EditorUtility.DisplayDialog(
                    _texts.ConfirmRemoveTitle ?? "Confirm",
                    _texts.ConfirmRemoveMsg,
                    _texts.BtnYes ?? "Yes",
                    _texts.BtnCancel ?? "Cancel"))
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("Remove Constraint Setup");
                    int undoGroup = Undo.GetCurrentGroup();

                    _setupService.RemoveConstraints(_bonePairs, _config);
                    _constraintsApplied = false;

                    Undo.CollapseUndoOperations(undoGroup);
                    EditorUtility.DisplayDialog("Complete", _texts.RemovedMsg, "OK");
                }
            }
            GUI.backgroundColor = oldBg;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }
    }
}


