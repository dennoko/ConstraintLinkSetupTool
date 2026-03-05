using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Tiloop.ConstraintLinkSetupTool.Core.Models;
using Tiloop.ConstraintLinkSetupTool.Core.Services;

namespace Tiloop.ConstraintLinkSetupTool.UI
{
    [System.Serializable]
    public class LocalizationData
    {
        public string WindowTitle;
        public string Settings;
        public string Language;
        public string Step1Title;
        public string Step1Desc;
        public string Step2Title;
        public string Step2Desc;
        public string Step3Title;
        
        public string AvatarBaseBone;
        public string ProstheticBaseBone;
        
        public string AdvancedFoldout;
        public string SideMode;
        public string SideAuto;
        public string SideRight;
        public string SideLeft;
        
        public string MappingPreview;
        public string ColUse;
        public string ColAvatarBone;
        public string ColProstheticBone;
        public string LabelBase;
        public string NoMappingMsg;
        public string ErrorNoBaseBone;
        public string ConfirmRemoveTitle;
        public string ConfirmRemoveMsg;
        public string BtnYes;
        public string BtnCancel;
        public string CompleteMsg;
        public string RemovedMsg;
        public string ExecuteBtn;
        public string RevertBtn;
        public string ToggleEn;
    }

    public class ConstraintLinkSetupToolWindow : EditorWindow
    {
        private SetupConfig _config = new SetupConfig();
        private List<BonePair> _bonePairs = new List<BonePair>();

        private BoneMappingService _mappingService;
        private ConstraintSetupService _setupService;

        private Vector2 _scrollPos;
        private bool _showAdvancedSettings = false;
        private bool _debugMode = false;

        // UI Styles
        private GUIStyle _headerStyle;
        private GUIStyle _stepBoxStyle;
        private GUIStyle _stepTitleStyle;
        private GUIStyle _boldHelpBoxStyle;

        // Localization
        private bool _isEnglish = false;
        private LocalizationData _texts;
        private string _basePath;

        [MenuItem("dennokoworks/Constraint Link Setup Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConstraintLinkSetupToolWindow>("Constraint Link Setup Tool");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnEnable()
        {
            _mappingService = new BoneMappingService();
            _setupService = new ConstraintSetupService();
            
            _basePath = "Assets/Editor/ConstraintLinkSetupTool/Docs/Lang";
            
            _isEnglish = EditorPrefs.GetBool("ConstraintLink_Lang_EN", false);
            LoadLanguage();
        }

        private void LoadLanguage()
        {
            string fileName = _isEnglish ? "en.json" : "ja.json";
            string path = Path.Combine(Application.dataPath, _basePath.Replace("Assets/", ""), fileName);

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
                margin = new RectOffset(0, 0, 10, 10)
            };

            _stepBoxStyle = new GUIStyle("HelpBox")
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 5, 10)
            };

            _stepTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                margin = new RectOffset(0, 0, 0, 5)
            };
            
            _boldHelpBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI()
        {
            if (_texts == null) LoadLanguage();
            InitializeStyles();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(_texts.WindowTitle, _headerStyle);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            _isEnglish = EditorGUILayout.ToggleLeft(" English", _isEnglish, GUILayout.Width(70));
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("ConstraintLink_Lang_EN", _isEnglish);
                LoadLanguage();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            DrawStep1BaseBoneSetup();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateAutoMapping();
            }

            DrawStep2AutoMapping();
            DrawStep3Execute();
        }

        private void UpdateAutoMapping()
        {
            if (_config.TargetAvatarBaseBone != null && _config.TargetProstheticBaseBone != null)
            {
                _mappingService.DebugMode = _debugMode;
                _bonePairs = _mappingService.MatchBones(
                    _config.TargetAvatarRoot, _config.TargetProstheticRoot,
                    _config.TargetAvatarBaseBone, _config.TargetProstheticBaseBone,
                    _config.PartSideMode);
            }
            else
            {
                if (_bonePairs != null) _bonePairs.Clear();
            }
        }

        private void DrawStep1BaseBoneSetup()
        {
            EditorGUILayout.BeginVertical(_stepBoxStyle);
            GUILayout.Label(_texts.Step1Title, _stepTitleStyle);
            EditorGUILayout.HelpBox(_texts.Step1Desc, MessageType.Info);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(_texts.AvatarBaseBone, EditorStyles.boldLabel, GUILayout.Height(30));
            _config.TargetAvatarBaseBone = (Transform)EditorGUILayout.ObjectField(_config.TargetAvatarBaseBone, typeof(Transform), true);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(_texts.ProstheticBaseBone, EditorStyles.boldLabel, GUILayout.Height(30));
            _config.TargetProstheticBaseBone = (Transform)EditorGUILayout.ObjectField(_config.TargetProstheticBaseBone, typeof(Transform), true);

            EditorGUILayout.Space(5);
            
            // Advanced Settings Foldout
            _showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, _texts.AdvancedFoldout, true, EditorStyles.foldoutHeader);
            if (_showAdvancedSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.Space(5);
                
                string[] sideOptions = { _texts.SideAuto, _texts.SideRight, _texts.SideLeft };
                _config.PartSideMode = (SideMode)EditorGUILayout.Popup(_texts.SideMode, (int)_config.PartSideMode, sideOptions);
                
                EditorGUILayout.Space(5);
                
                _debugMode = EditorGUILayout.ToggleLeft("Debug Mode (Log/)", _debugMode, EditorStyles.miniLabel);
                
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStep2AutoMapping()
        {
            EditorGUILayout.BeginVertical(_stepBoxStyle);
            GUILayout.Label(_texts.Step2Title, _stepTitleStyle);
            EditorGUILayout.HelpBox(_texts.Step2Desc, MessageType.Info);


            EditorGUILayout.Space(10);
            GUILayout.Label(_texts.MappingPreview, EditorStyles.boldLabel);

            // Table Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label(_texts.ColUse, EditorStyles.miniLabel, GUILayout.Width(30));
            GUILayout.Label(_texts.ColAvatarBone, EditorStyles.miniLabel, GUILayout.MinWidth(80));
            GUILayout.Label("", GUILayout.Width(25)); // Center spacer
            GUILayout.Label(_texts.ColProstheticBone, EditorStyles.miniLabel, GUILayout.MinWidth(80));
            GUILayout.Label("", GUILayout.Width(40)); // Right spacer
            EditorGUILayout.EndHorizontal();

            // Scroll View for Mappings
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "box", GUILayout.MinHeight(150), GUILayout.MaxHeight(250));
            
            if (_bonePairs != null && _bonePairs.Count > 0)
            {
                for (int i = 0; i < _bonePairs.Count; i++)
                {
                    var pair = _bonePairs[i];
                    EditorGUILayout.BeginHorizontal();
                    
                    GUILayout.Space(5);

                    pair.ApplyConstraint = EditorGUILayout.Toggle(pair.ApplyConstraint, GUILayout.Width(20));
                    pair.AvatarBone = (Transform)EditorGUILayout.ObjectField(pair.AvatarBone, typeof(Transform), true, GUILayout.MinWidth(80));
                    
                    GUILayout.Label("↔", GUILayout.Width(20));
                    
                    pair.ProstheticBone = (Transform)EditorGUILayout.ObjectField(pair.ProstheticBone, typeof(Transform), true, GUILayout.MinWidth(80));
                    
                    if (pair.IsBaseBone)
                    {
                        GUILayout.Label(_texts.LabelBase, EditorStyles.miniBoldLabel, GUILayout.Width(40));
                    }
                    else
                    {
                        GUILayout.Space(44);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox(_texts.NoMappingMsg, MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawStep3Execute()
        {
            EditorGUILayout.BeginVertical(_stepBoxStyle);
            GUILayout.Label(_texts.Step3Title, _stepTitleStyle);

            EditorGUILayout.Space(5);

            GUI.enabled = _config.IsValid() && _bonePairs != null && _bonePairs.Count > 0;
            
            Color oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f); // Greenish Execute Button
            if (GUILayout.Button(_texts.ExecuteBtn, GUILayout.Height(40)))
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Constraint Setup");
                int undoGroup = Undo.GetCurrentGroup();

                _setupService.ApplyConstraints(_bonePairs, _config);

                Undo.CollapseUndoOperations(undoGroup);
                EditorUtility.DisplayDialog("Complete", _texts.CompleteMsg, "OK");
            }
            GUI.backgroundColor = oldBg;
            GUI.enabled = true;
            
            EditorGUILayout.Space(10);
            
            GUI.enabled = _bonePairs != null && _bonePairs.Count > 0;
            
            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f); // Reddish Revert Button
            if (GUILayout.Button(_texts.RevertBtn, GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog(_texts.ConfirmRemoveTitle, _texts.ConfirmRemoveMsg, _texts.BtnYes, _texts.BtnCancel))
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("Remove Constraint Setup");
                    int undoGroup = Undo.GetCurrentGroup();

                    _setupService.RemoveConstraints(_bonePairs, _config);

                    Undo.CollapseUndoOperations(undoGroup);
                    EditorUtility.DisplayDialog("Complete", _texts.RemovedMsg, "OK");
                }
            }
            GUI.backgroundColor = oldBg;
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }
    }
}


