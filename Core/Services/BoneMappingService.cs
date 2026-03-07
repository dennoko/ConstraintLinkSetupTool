using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Tiloop.ConstraintLinkSetupTool.Core.Models;
using Tiloop.ConstraintLinkSetupTool.Core.Services.Interfaces;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services
{
    public class BoneMappingService : IBoneMappingService
    {
        /// <summary>
        /// デバッグモード: 有効時にマッピング結果をLog/に保存
        /// </summary>
        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// グローバル探索によるボーンマッチング
        /// </summary>
        public List<BonePair> MatchBones(
            Transform avatarRoot, Transform prostheticRoot,
            Transform avatarBaseBone, Transform prostheticBaseBone,
            SideMode sideMode)
        {
            var bonePairs = new List<BonePair>();
            var debugLog = DebugMode ? new StringBuilder() : null;

            if (avatarRoot == null || prostheticRoot == null ||
                avatarBaseBone == null || prostheticBaseBone == null)
            {
                return bonePairs;
            }

            // --- 左右判定 ---
            bool isRightSide = true;
            if (sideMode == SideMode.Auto)
            {
                isRightSide = DetectSide(avatarBaseBone);
                if (DebugMode)
                    Debug.Log($"[ConstraintLink] Auto Detected Side: {(isRightSide ? "Right" : "Left")} based on {avatarBaseBone.name}");
            }
            else
            {
                isRightSide = (sideMode == SideMode.Right);
            }

            debugLog?.AppendLine("=== Constraint Link Mapping Debug Log ===");
            debugLog?.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            debugLog?.AppendLine($"Avatar Root: {avatarRoot.name}");
            debugLog?.AppendLine($"Prosthetic Root: {prostheticRoot.name}");
            debugLog?.AppendLine($"Avatar Base Bone: {avatarBaseBone.name}");
            debugLog?.AppendLine($"Prosthetic Base Bone: {prostheticBaseBone.name}");
            debugLog?.AppendLine($"Side Mode: {sideMode} → Detected: {(isRightSide ? "Right" : "Left")}");
            debugLog?.AppendLine();

            // --- Step 1: ベースボーン同士をアンカーとしてペアリング ---
            bonePairs.Add(new BonePair(avatarBaseBone, prostheticBaseBone, isBaseBone: true));
            debugLog?.AppendLine($"[BASE] {prostheticBaseBone.name} ↔ {avatarBaseBone.name} (anchor)");

            // --- Step 2: アバター側の全ボーンをフラット化してキャッシュ ---
            var avatarAllBones = avatarRoot.GetComponentsInChildren<Transform>(true);

            var avatarBoneByHumanBone = new Dictionary<HumanBodyBones, Transform>();
            var avatarBoneByNormalizedName = new Dictionary<string, List<Transform>>();

            foreach (var bone in avatarAllBones)
            {
                var humanBone = BoneNameMatcher.TryMatchBone(bone.name);
                if (humanBone.HasValue && !avatarBoneByHumanBone.ContainsKey(humanBone.Value))
                {
                    int boneSide = BoneNameMatcher.GetSideIndicator(bone.name);
                    if (boneSide == 0 || (boneSide > 0 == isRightSide))
                    {
                        avatarBoneByHumanBone[humanBone.Value] = bone;
                    }
                }

                var normalized = BoneNameMatcher.NormalizeBoneName(bone.name);
                if (!string.IsNullOrEmpty(normalized))
                {
                    if (!avatarBoneByNormalizedName.TryGetValue(normalized, out var list))
                    {
                        list = new List<Transform>();
                        avatarBoneByNormalizedName[normalized] = list;
                    }
                    list.Add(bone);
                }
            }

            debugLog?.AppendLine();
            debugLog?.AppendLine($"--- Avatar Bone Cache: {avatarBoneByHumanBone.Count} HumanBone entries, {avatarBoneByNormalizedName.Count} normalized name entries ---");
            debugLog?.AppendLine();
            debugLog?.AppendLine("=== Matching Results ===");

            // --- Step 3: 義手側のベースボーン以下のみを走査してグローバルマッチング ---
            // ※義手自体のHipsやSpineといった、アンカーより上位の構造ボーンやMeshを除外するため
            var prostheticAllBones = prostheticBaseBone.GetComponentsInChildren<Transform>(true);
            var matchedAvatarBones = new HashSet<Transform>();

            var prostheticToAvatarMap = new Dictionary<Transform, Transform>();

            var unmatchedProsthetics = new List<Transform>();
            int nameMatchCount = 0;

            foreach (var pBone in prostheticAllBones)
            {
                Transform matched = null;
                string matchMethod = "NONE";

                // --- 3a: HumanBodyBones 辞書による高精度マッチング ---
                var pHumanBone = BoneNameMatcher.TryMatchBone(pBone.name);
                if (pHumanBone.HasValue)
                {
                    if (avatarBoneByHumanBone.TryGetValue(pHumanBone.Value, out var avatarBone)
                        && !matchedAvatarBones.Contains(avatarBone))
                    {
                        matched = avatarBone;
                        matchMethod = $"DICT({pHumanBone.Value})";
                    }
                }

                // --- 3b: 正規化名による直接マッチング ---
                if (matched == null)
                {
                    var pNormalized = BoneNameMatcher.NormalizeBoneName(pBone.name);
                    if (!string.IsNullOrEmpty(pNormalized) &&
                        avatarBoneByNormalizedName.TryGetValue(pNormalized, out var candidates))
                    {
                        foreach (var candidate in candidates)
                        {
                            if (matchedAvatarBones.Contains(candidate))
                                continue;

                            int candidateSide = BoneNameMatcher.GetSideIndicator(candidate.name);
                            if (candidateSide == 0 || (candidateSide > 0 == isRightSide))
                            {
                                matched = candidate;
                                matchMethod = $"NAME({pNormalized})";
                                break;
                            }
                        }
                    }
                }

                if (matched != null)
                {
                    bonePairs.Add(new BonePair(matched, pBone, isBaseBone: false));
                    matchedAvatarBones.Add(matched);
                    prostheticToAvatarMap[pBone] = matched;
                    nameMatchCount++;

                    debugLog?.AppendLine($"  [OK  ] {pBone.name,-35} ↔ {matched.name,-35} method={matchMethod}");
                }
                else
                {
                    unmatchedProsthetics.Add(pBone);
                    var pNorm = BoneNameMatcher.NormalizeBoneName(pBone.name);
                    var pHuman = pHumanBone.HasValue ? pHumanBone.Value.ToString() : "none";
                    debugLog?.AppendLine($"  [MISS] {pBone.name,-35} normalized=\"{pNorm}\" humanBone={pHuman}");
                }
            }

            // --- Step 4: フォールバック (階層ベース) ---
            debugLog?.AppendLine();
            debugLog?.AppendLine("=== Fallback (Hierarchy-based) ===");
            int fallbackMatchCount = 0;

            foreach (var pBone in unmatchedProsthetics)
            {
                if (pBone.parent == null)
                    continue;

                if (prostheticToAvatarMap.TryGetValue(pBone.parent, out var avatarParent))
                {
                    int myIndex = GetChildIndex(pBone);

                    if (myIndex >= 0 && myIndex < avatarParent.childCount)
                    {
                        var avatarChild = avatarParent.GetChild(myIndex);
                        if (!matchedAvatarBones.Contains(avatarChild))
                        {
                            int childSide = BoneNameMatcher.GetSideIndicator(avatarChild.name);
                            if (childSide == 0 || (childSide > 0 == isRightSide))
                            {
                                bonePairs.Add(new BonePair(avatarChild, pBone, isBaseBone: false));
                                matchedAvatarBones.Add(avatarChild);
                                prostheticToAvatarMap[pBone] = avatarChild;
                                fallbackMatchCount++;

                                debugLog?.AppendLine($"  [FALL] {pBone.name,-35} ↔ {avatarChild.name,-35} parentIdx={myIndex}");
                            }
                        }
                    }
                }
            }

            // --- Summary ---
            debugLog?.AppendLine();
            debugLog?.AppendLine("=== Summary ===");
            debugLog?.AppendLine($"Total Pairs: {bonePairs.Count}");
            debugLog?.AppendLine($"  Name-based: {nameMatchCount}");
            debugLog?.AppendLine($"  Fallback:   {fallbackMatchCount}");
            debugLog?.AppendLine($"  Unmatched:  {unmatchedProsthetics.Count - fallbackMatchCount}");
            debugLog?.AppendLine($"  Base Bone:  1");

            if (DebugMode)
                Debug.Log($"[ConstraintLink] Matched {bonePairs.Count} bone pairs (Name: {nameMatchCount}, Fallback: {fallbackMatchCount})");

            // --- Write debug log to file ---
            if (DebugMode && debugLog != null)
            {
                WriteDebugLog(debugLog.ToString());
            }

            return bonePairs;
        }

        /// <summary>
        /// デバッグログをLog/ディレクトリに書き出す
        /// </summary>
        private void WriteDebugLog(string content)
        {
            try
            {
                string logDir = Path.Combine(
                    Application.dataPath,
                    "Editor/ConstraintLinkSetupTool/Log");

                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                string fileName = $"mapping_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(logDir, fileName);

                File.WriteAllText(filePath, content, Encoding.UTF8);
                Debug.Log($"[ConstraintLink] Debug log saved: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ConstraintLink] Failed to write debug log: {e.Message}");
            }
        }

        private bool DetectSide(Transform baseBone)
        {
            int indicator = BoneNameMatcher.GetSideIndicator(baseBone.name);
            if (indicator != 0) return indicator > 0;

            if (baseBone.parent != null)
            {
                indicator = BoneNameMatcher.GetSideIndicator(baseBone.parent.name);
                if (indicator != 0) return indicator > 0;
            }

            if (baseBone.childCount > 0)
            {
                indicator = BoneNameMatcher.GetSideIndicator(baseBone.GetChild(0).name);
                if (indicator != 0) return indicator > 0;
            }

            Transform root = baseBone.root;
            Vector3 relativePos = root.InverseTransformPoint(baseBone.position);
            
            if (relativePos.x < -0.01f) return true;
            if (relativePos.x > 0.01f) return false;

            return true;
        }

        private int GetChildIndex(Transform child)
        {
            if (child.parent == null) return -1;
            for (int i = 0; i < child.parent.childCount; i++)
            {
                if (child.parent.GetChild(i) == child) return i;
            }
            return -1;
        }
    }
}

