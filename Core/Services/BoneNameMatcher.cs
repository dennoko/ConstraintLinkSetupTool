using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services
{
    /// <summary>
    /// ボーン名の正規化とマッチングを行うユーティリティクラス
    /// </summary>
    public static class BoneNameMatcher
    {
        private static readonly Regex EndNumberPattern = new Regex(@"[_\.][0-9]+$");
        private static readonly Regex VrmBonePattern = new Regex(@"^([LRC])_(.*)$");
        private static readonly Regex SideSuffixPattern = new Regex(@"[_\.]([LR])$", RegexOptions.IgnoreCase);

        /// <summary>
        /// ボーン名を正規化する
        /// 大文字小文字、数字、スペース、アンダースコア、ドットを統一的に処理
        /// L/R短縮形はleft/rightにフルスペル展開してから正規化する
        /// </summary>
        public static string NormalizeBoneName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            // 小文字化
            name = name.ToLowerInvariant();

            // "Bone_" プレフィックスを除去
            if (name.StartsWith("bone_"))
                name = name.Substring(5);

            // L/R 短縮形をフルスペルに展開（セパレータ付きのもののみ対象）
            // 例: _l, .l, l_, l. → left / _r, .r, r_, r. → right
            // 末尾パターン: shoulder_l, shoulder.l
            name = Regex.Replace(name, @"[_\.]l$", "left");
            name = Regex.Replace(name, @"[_\.]r$", "right");
            // 先頭パターン: l_shoulder, l.shoulder
            name = Regex.Replace(name, @"^l[_\.]", "left");
            name = Regex.Replace(name, @"^r[_\.]", "right");
            // 中間パターン: arm_l_001 etc. (数字の前)
            name = Regex.Replace(name, @"[_\.]l[_\.]", "left");
            name = Regex.Replace(name, @"[_\.]r[_\.]", "right");

            // 数字、スペース、アンダースコア、ドットを除去
            name = Regex.Replace(name, @"[0-9 ._]", "");

            return name;
        }

        /// <summary>
        /// ボーン名からHumanBodyBonesを推定する
        /// </summary>
        /// <param name="boneName">対象のボーン名</param>
        /// <returns>マッチしたボーン、見つからない場合はnull</returns>
        public static HumanBodyBones? TryMatchBone(string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
                return null;

            var normalized = NormalizeBoneName(boneName);

            if (HumanoidBonePatterns.NameToBoneMap.TryGetValue(normalized, out var bones) && bones.Count > 0)
            {
                return bones[0];
            }

            return null;
        }

        /// <summary>
        /// 2つのボーン名が同じHumanBodyBonesを指しているか、または正規化名が一致するか判定する（フォールバック用）
        /// </summary>
        public static bool TryMatchBone(string avatarBoneName, string prostheticBoneName)
        {
            var bone1 = TryMatchBone(avatarBoneName);
            var bone2 = TryMatchBone(prostheticBoneName);
            
            if (bone1.HasValue && bone2.HasValue)
            {
                return bone1.Value == bone2.Value;
            }

            return NormalizeBoneName(avatarBoneName) == NormalizeBoneName(prostheticBoneName);
        }

        /// <summary>
        /// ボーン名からマッチする可能性のある全てのHumanBodyBonesを取得
        /// </summary>
        public static List<HumanBodyBones> GetPossibleBones(string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
                return new List<HumanBodyBones>();

            var normalized = NormalizeBoneName(boneName);

            if (HumanoidBonePatterns.NameToBoneMap.TryGetValue(normalized, out var bones))
            {
                return new List<HumanBodyBones>(bones);
            }

            return new List<HumanBodyBones>();
        }

        /// <summary>
        /// 正規化されたボーン名がヒューマノイドボーンとして認識可能かどうか
        /// </summary>
        public static bool IsRecognizedBoneName(string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
                return false;

            var normalized = NormalizeBoneName(boneName);
            return HumanoidBonePatterns.AllNormalizedBoneNames.Contains(normalized);
        }

        /// <summary>
        /// ボーン名からプレフィックスとサフィックスを推定する
        /// </summary>
        public static bool TryInferPrefixSuffix(string boneName, string knownBonePart, 
            out string prefix, out string suffix)
        {
            prefix = string.Empty;
            suffix = string.Empty;

            if (string.IsNullOrEmpty(boneName) || string.IsNullOrEmpty(knownBonePart))
                return false;

            // 大文字小文字を無視して検索
            int index = boneName.IndexOf(knownBonePart, System.StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return false;

            prefix = boneName.Substring(0, index);
            suffix = boneName.Substring(index + knownBonePart.Length);
            return true;
        }

        /// <summary>
        /// プレフィックスとサフィックスを除去してベース名を取得
        /// </summary>
        public static string StripPrefixSuffix(string boneName, string prefix, string suffix)
        {
            if (string.IsNullOrEmpty(boneName))
                return boneName;

            if (!string.IsNullOrEmpty(prefix) && boneName.StartsWith(prefix))
            {
                boneName = boneName.Substring(prefix.Length);
            }

            if (!string.IsNullOrEmpty(suffix) && boneName.EndsWith(suffix))
            {
                boneName = boneName.Substring(0, boneName.Length - suffix.Length);
            }

            return boneName;
        }

        /// <summary>
        /// ボーン名が左右のサイド情報を含むかどうかを判定
        /// </summary>
        public static bool HasSideIndicator(string boneName, out bool isLeft)
        {
            isLeft = false;
            if (string.IsNullOrEmpty(boneName))
                return false;

            var lower = boneName.ToLowerInvariant();

            // Left/Rightの文字列を含むか
            if (lower.Contains("left") || lower.Contains("_l") || lower.EndsWith(".l") || 
                lower.StartsWith("l_") || lower.StartsWith("l."))
            {
                isLeft = true;
                return true;
            }

            if (lower.Contains("right") || lower.Contains("_r") || lower.EndsWith(".r") || 
                lower.StartsWith("r_") || lower.StartsWith("r."))
            {
                isLeft = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 名前から左右どちらに属しているかを判定する
        /// 1 = Right, -1 = Left, 0 = Unknown/Center
        /// </summary>
        public static int GetSideIndicator(string name)
        {
            if (HasSideIndicator(name, out bool isLeft))
            {
                return isLeft ? -1 : 1;
            }
            return 0;
        }
    }
}
