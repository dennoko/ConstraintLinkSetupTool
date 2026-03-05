using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using Tiloop.ConstraintLinkSetupTool.Core.Models;
using Tiloop.ConstraintLinkSetupTool.Core.Services.Interfaces;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services
{
    public class ConstraintSetupService : IConstraintSetupService
    {
        public void ApplyConstraints(List<BonePair> bonePairs, SetupConfig config)
        {
            if (config == null || !config.IsValid() || bonePairs == null || bonePairs.Count == 0)
                return;

            // 1. 義手（パーツ）をアバターのルート直下に配置する。
            if (config.TargetProstheticRoot.parent != config.TargetAvatarRoot)
            {
                Undo.SetTransformParent(config.TargetProstheticRoot, config.TargetAvatarRoot, "Reparent Prosthetic");
                // 配置（座標）自体はユーザーが既に行った前提とする
            }

            foreach (var pair in bonePairs)
            {
                if (!pair.ApplyConstraint || pair.AvatarBone == null || pair.ProstheticBone == null)
                    continue;

                // 2 & 4. 義手の対象ボーンから既存のコンストレイントを取り除くなどの初期化処理
                // ここでは必要に応じて重複追加の防止をチェックする。

                if (pair.IsBaseBone)
                {
                    // ベースボーン：義手全体の追従にParentConstraintなどを使用
                    // アバターのターゲットボーン（例：右肩）に対して、義手の原点を追従させる。
                    SetupBaseConstraint(pair.ProstheticBone, pair.AvatarBone, config.EnablePositionConstraint);
                }
                else
                {
                    // 子ボーン：Rotation Constraint で回転のみ同期
                    SetupRotationConstraint(pair.ProstheticBone, pair.AvatarBone);
                }
            }
        }

        public void RemoveConstraints(List<BonePair> bonePairs, SetupConfig config)
        {
            if (bonePairs == null) return;
            
            // Undoに対応できるようにするため、基本的にDestroyImmediateではなくUndoオブジェクトを使用するのがEditor的（簡易的には以下）
            foreach (var pair in bonePairs)
            {
                if (pair.ProstheticBone == null) continue;

                var rotConstraint = pair.ProstheticBone.GetComponent<RotationConstraint>();
                if (rotConstraint != null)
                {
                    Undo.DestroyObjectImmediate(rotConstraint);
                }
                
                var posConstraint = pair.ProstheticBone.GetComponent<PositionConstraint>();
                if (posConstraint != null)
                {
                    Undo.DestroyObjectImmediate(posConstraint);
                }

                if (pair.IsBaseBone)
                {
                    var parentConstraint = pair.ProstheticBone.GetComponent<ParentConstraint>();
                    if (parentConstraint != null)
                    {
                        Undo.DestroyObjectImmediate(parentConstraint);
                    }
                }
            }
        }

        private void SetupRotationConstraint(Transform prostheticBone, Transform avatarScaleBone)
        {
            // すでにある場合は破棄または上書き
            var rotConstraint = prostheticBone.GetComponent<RotationConstraint>();
            if (rotConstraint == null)
            {
                // Componentを追加（Undo対応）
                rotConstraint = Undo.AddComponent<RotationConstraint>(prostheticBone.gameObject);
            }

            // オフセット計算をセットアップする（Zeroed out状態を作る）
            // コンストレイントソースを追加
            ConstraintSource source = new ConstraintSource
            {
                sourceTransform = avatarScaleBone,
                weight = 1.0f
            };
            
            // ソース追加前にリストをクリア（念のため）
            for(int i = rotConstraint.sourceCount - 1; i >= 0; i--)
            {
                rotConstraint.RemoveSource(i);
            }
            rotConstraint.AddSource(source);

            // Activateして現在の姿勢(Offset)をロックする
            // AtRest に現在のLocal Rotationを保存
            rotConstraint.rotationAtRest = prostheticBone.localEulerAngles;
            
            // 現在の姿勢からターゲットとの差分(Offset)を計算して保持
            Quaternion avatarRot = avatarScaleBone.rotation;
            Quaternion prostheticRot = prostheticBone.rotation;
            // new_rot = avatarRot * rotationOffset
            Quaternion offset = Quaternion.Inverse(avatarRot) * prostheticRot;
            rotConstraint.rotationOffset = offset.eulerAngles;
            
            // Active ＆ Lock処理
            rotConstraint.locked = true;
            rotConstraint.constraintActive = true;
        }

        private void SetupBaseConstraint(Transform prostheticBone, Transform avatarBone, bool enablePosition)
        {
            if (enablePosition)
            {
                // 位置も回転も同期（従来の Parent Constraint 相当、または Position + Rotation 両方がけ）
                // VRChatのSDK3環境では ParentConstraint をそのまま使うのが一般的ですが、
                // 細かい制御のために Position と Rotation を分けることもあります。今回は Parent Constraint を使用
                var pConstraint = prostheticBone.GetComponent<ParentConstraint>();
                if (pConstraint == null)
                {
                    pConstraint = Undo.AddComponent<ParentConstraint>(prostheticBone.gameObject);
                }

                ConstraintSource source = new ConstraintSource
                {
                    sourceTransform = avatarBone,
                    weight = 1.0f
                };

                for (int i = pConstraint.sourceCount - 1; i >= 0; i--)
                {
                    pConstraint.RemoveSource(i);
                }
                pConstraint.AddSource(source);

                pConstraint.translationAtRest = prostheticBone.localPosition;
                pConstraint.rotationAtRest = prostheticBone.localEulerAngles;
                
                // 現在の姿勢からターゲットとの差分(Offset)を計算して保持
                Vector3 offsetPos = avatarBone.InverseTransformPoint(prostheticBone.position);
                Quaternion offsetRot = Quaternion.Inverse(avatarBone.rotation) * prostheticBone.rotation;
                
                pConstraint.SetTranslationOffset(0, offsetPos);
                pConstraint.SetRotationOffset(0, offsetRot.eulerAngles);

                pConstraint.locked = true;
                pConstraint.constraintActive = true;
            }
            else
            {
                // 回転のみ同期 (Rotation Constraint のみ) 
                // Position は動かさず、指定された位置を維持する
                SetupRotationConstraint(prostheticBone, avatarBone);
            }
        }
    }
}

