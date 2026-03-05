using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services
{
    public static class BoneNameMatcher
    {
        // 判定用の接頭辞・接尾辞パターン
        private static readonly Regex LeftPattern = new Regex(@"(?i)(\b|_|\.)(L|Left)(\b|_|\.|[0-9])");
        private static readonly Regex RightPattern = new Regex(@"(?i)(\b|_|\.)(R|Right)(\b|_|\.|[0-9])");

        /// <summary>
        /// ボーン名から余分な装飾を取り除き、小文字化して正規化する
        /// </summary>
        public static string NormalizeBoneName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return "";

            string name = rawName;

            // "Bone_" などの接頭辞を除去
            name = Regex.Replace(name, @"(?i)^bone_", "");

            // L/R などのサイドフラグを除去 (左/右は別のロジックで抽出する前提)
            name = LeftPattern.Replace(name, "");
            name = RightPattern.Replace(name, "");

            // スペース、アンダースコア、ドット、数字を除去
            name = Regex.Replace(name, @"[\s_\.\d]+", "");

            return name.ToLowerInvariant();
        }

        /// <summary>
        /// 名前から左右どちらに属しているかを判定する
        /// 1 = Right, -1 = Left, 0 = Unknown/Center
        /// </summary>
        public static int GetSideIndicator(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;

            if (RightPattern.IsMatch(name)) return 1;
            if (LeftPattern.IsMatch(name)) return -1;

            return 0; // 中央、あるいは判定不能
        }

        /// <summary>
        /// HumanBodyBonesなどとの一致を試みる（簡易的）
        /// </summary>
        public static bool TryMatchBone(string avatarBoneName, string prostheticBoneName)
        {
            return NormalizeBoneName(avatarBoneName) == NormalizeBoneName(prostheticBoneName);
        }
    }
}
