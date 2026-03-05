using System;
using System.Collections.Generic;
using UnityEngine;
using Tiloop.ConstraintLinkSetupTool.Core.Models;
using Tiloop.ConstraintLinkSetupTool.Core.Services.Interfaces;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services
{
    public class BoneMappingService : IBoneMappingService
    {
        public List<BonePair> MatchBones(Transform avatarBaseBone, Transform prostheticBaseBone, SideMode sideMode)
        {
            var bonePairs = new List<BonePair>();
            if (avatarBaseBone == null || prostheticBaseBone == null)
            {
                return bonePairs;
            }

            bool isRightSide = true; // デフォルト
            if (sideMode == SideMode.Auto)
            {
                isRightSide = DetectSide(avatarBaseBone);
                Debug.Log($"[ConstraintLink] Auto Detected Side: {(isRightSide ? "Right" : "Left")} based on {avatarBaseBone.name}");
            }
            else
            {
                isRightSide = (sideMode == SideMode.Right);
            }

            // ベースボーンをペアとして登録
            bonePairs.Add(new BonePair(avatarBaseBone, prostheticBaseBone, isBaseBone: true));

            // 再帰的に子ボーンを対応付け
            MapChildrenRecursive(avatarBaseBone, prostheticBaseBone, bonePairs, isRightSide);

            return bonePairs;
        }

        private bool DetectSide(Transform baseBone)
        {
            // 1. ボーン名（およびその直接の親や子）からの判定
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

            // 2. 空間座標（X位置）による判定 (フォールバック)
            // 原点(ルート)から見てどちらにあるか。通常はUnityでは右腕がXマイナス側、左腕がXプラス側ですが
            // アバターによってローカル原点が異なる可能性があるため、ルートの Transform を基準とした LocalPosition のXを確認します。
            Transform root = baseBone.root;
            Vector3 relativePos = root.InverseTransformPoint(baseBone.position);
            
            // 標準的な T-Pose や A-Pose では Right は X < 0、Left は X > 0 にいることが多い
            if (relativePos.x < -0.01f) return true; // Right
            if (relativePos.x > 0.01f) return false; // Left

            // 全てダメならデフォルト(右)
            return true;
        }

        private void MapChildrenRecursive(Transform avatarBone, Transform prostheticBone, List<BonePair> bonePairs, bool isRightSideTarget)
        {
            // プロステティック側（義手側）の子ボーンをベースに処理
            for (int i = 0; i < prostheticBone.childCount; i++)
            {
                Transform prostheticChild = prostheticBone.GetChild(i);
                Transform matchedAvatarChild = null;

                // 1. 名前ベースの推測: BoneNameMatcher による推測を優先利用
                for (int j = 0; j < avatarBone.childCount; j++)
                {
                    Transform avatarChild = avatarBone.GetChild(j);
                    
                    // 名前のマッチング
                    if (BoneNameMatcher.TryMatchBone(avatarChild.name, prostheticChild.name))
                    {
                        // 左右の判定を追加: 対象サイドと同じサイドか、中央系のボーンであれば許可
                        int childSide = BoneNameMatcher.GetSideIndicator(avatarChild.name);
                        
                        // 判定不能(0) または 指定サイドと一致する場合のみ採用 (逆サイドを拾うのを防ぐ)
                        if (childSide == 0 || (childSide > 0 == isRightSideTarget))
                        {
                            matchedAvatarChild = avatarChild;
                            break;
                        }
                    }
                }

                // 2. 階層ベースの推測: 該当する名前が見つからなければ、インデックスで一致を試みる
                if (matchedAvatarChild == null)
                {
                    if (i < avatarBone.childCount)
                    {
                        matchedAvatarChild = avatarBone.GetChild(i);
                    }
                }

                if (matchedAvatarChild != null)
                {
                    bonePairs.Add(new BonePair(matchedAvatarChild, prostheticChild, isBaseBone: false));
                    
                    // 更に深く走査
                    MapChildrenRecursive(matchedAvatarChild, prostheticChild, bonePairs, isRightSideTarget);
                }
            }
        }
    }
}
