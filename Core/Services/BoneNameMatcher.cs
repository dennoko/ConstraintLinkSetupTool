using System.Text.RegularExpressions;
using UnityEngine;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services
{
    /// <summary>
    /// ボーン名の正規化とマッチングを行うユーティリティクラス
    /// </summary>
    public static class BoneNameMatcher
    {

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

            // セパレータ（スペース、アンダースコア、ドット）を除去
            name = Regex.Replace(name, @"[ ._]", "");

            // 3桁以上の連続数字はインスタンス番号として除去（例: _001 → 削除）
            name = Regex.Replace(name, @"\d{3,}", "");

            // 1〜2桁の数字は先頭ゼロを正規化（例: 01 → 1, 02 → 2）
            // これにより spine01→spine1, spine02→spine2 が正しく区別される
            name = Regex.Replace(name, @"0+([1-9])", "$1");

            // 残留するゼロのみの数字列を除去
            name = Regex.Replace(name, @"0+", "");

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
