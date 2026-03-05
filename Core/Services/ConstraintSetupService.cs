using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using nadena.dev.modular_avatar.core;
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
            }

            // 2. 義手のルートに MA Bone Proxy を設定（アバター側の指定ベースボーンをアンカーとする）
            SetupMABoneProxy(config.TargetProstheticRoot, config.TargetAvatarBaseBone);

            foreach (var pair in bonePairs)
            {
                if (!pair.ApplyConstraint || pair.AvatarBone == null || pair.ProstheticBone == null)
                    continue;

                // 全てのボーン（ベースボーン含む）に対して Rotation Constraint で回転のみ同期
                SetupRotationConstraint(pair.ProstheticBone, pair.AvatarBone);
            }
        }

        public void RemoveConstraints(List<BonePair> bonePairs, SetupConfig config)
        {
            if (bonePairs == null) return;
            
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

                var parentConstraint = pair.ProstheticBone.GetComponent<ParentConstraint>();
                if (parentConstraint != null)
                {
                    Undo.DestroyObjectImmediate(parentConstraint);
                }
            }

            // MA Bone Proxy の削除
            if (config != null && config.TargetProstheticRoot != null)
            {
                var proxy = config.TargetProstheticRoot.GetComponent<ModularAvatarBoneProxy>();
                if (proxy != null)
                {
                    Undo.DestroyObjectImmediate(proxy);
                }
            }
        }

        private void SetupMABoneProxy(Transform root, Transform anchor)
        {
            var proxy = root.GetComponent<ModularAvatarBoneProxy>();
            if (proxy == null)
            {
                proxy = Undo.AddComponent<ModularAvatarBoneProxy>(root.gameObject);
            }

            proxy.target = anchor;
            proxy.attachmentMode = BoneProxyAttachmentMode.AsChildKeepWorldPose;
            
            EditorUtility.SetDirty(proxy);
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
    }
}

