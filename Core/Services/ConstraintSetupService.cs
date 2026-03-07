using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Constraint.Components;
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

            // 1. 義手をアバタールート直下に配置
            if (config.TargetProstheticRoot.parent != config.TargetAvatarRoot)
                Undo.SetTransformParent(config.TargetProstheticRoot, config.TargetAvatarRoot, "Reparent Prosthetic");

            // 2. 義手のベースボーンに MA Bone Proxy を設定
            SetupMABoneProxy(config.TargetProstheticBaseBone, config.TargetAvatarBaseBone);

            Transform prostheticRoot = config.TargetProstheticRoot;

            foreach (var pair in bonePairs)
            {
                if (!pair.ApplyConstraint || pair.AvatarBone == null || pair.ProstheticBone == null)
                    continue;

                // 内部参照コンストレイントがある場合はスキップ（設定を維持・重複防止）
                if (pair.HasInternalConstraint)
                    continue;

                SetupVRCRotationConstraint(pair.ProstheticBone, pair.AvatarBone, prostheticRoot);
            }
        }

        public void RemoveConstraints(List<BonePair> bonePairs, SetupConfig config)
        {
            if (bonePairs == null) return;

            foreach (var pair in bonePairs)
            {
                if (pair.ProstheticBone == null) continue;

                // 当ツールが追加した VRC Rotation Constraint（ソースが pair.AvatarBone）を削除
                foreach (var c in pair.ProstheticBone.GetComponents<VRCRotationConstraint>())
                {
                    if (SourceMatchesAvatarBone(c, pair.AvatarBone))
                        Undo.DestroyObjectImmediate(c);
                }

                // 旧バージョン互換: Unity RotationConstraint（ソースが pair.AvatarBone）も削除
                foreach (var c in pair.ProstheticBone.GetComponents<RotationConstraint>())
                {
                    if (SourceMatchesAvatarBone(c, pair.AvatarBone))
                        Undo.DestroyObjectImmediate(c);
                }
            }

            // MA Bone Proxy の削除
            if (config != null && config.TargetProstheticBaseBone != null)
            {
                var proxy = config.TargetProstheticBaseBone.GetComponent<ModularAvatarBoneProxy>();
                if (proxy != null)
                    Undo.DestroyObjectImmediate(proxy);
            }
        }

        /// <summary>
        /// VRC Rotation Constraintのいずれかのソースが指定のTransformと一致するか
        /// </summary>
        private static bool SourceMatchesAvatarBone(VRCRotationConstraint c, Transform avatarBone)
        {
            foreach (var src in c.Sources)
                if (src.SourceTransform == avatarBone) return true;
            return false;
        }

        /// <summary>
        /// Unity RotationConstraintのいずれかのソースが指定のTransformと一致するか
        /// </summary>
        private static bool SourceMatchesAvatarBone(RotationConstraint c, Transform avatarBone)
        {
            for (int i = 0; i < c.sourceCount; i++)
                if (c.GetSource(i).sourceTransform == avatarBone) return true;
            return false;
        }

        private void SetupMABoneProxy(Transform root, Transform anchor)
        {
            var proxy = root.GetComponent<ModularAvatarBoneProxy>();
            if (proxy == null)
                proxy = Undo.AddComponent<ModularAvatarBoneProxy>(root.gameObject);

            proxy.target = anchor;
            proxy.attachmentMode = BoneProxyAttachmentMode.AsChildKeepWorldPose;
            EditorUtility.SetDirty(proxy);
        }

        private void SetupVRCRotationConstraint(Transform prostheticBone, Transform avatarBone, Transform prostheticRoot)
        {
            // AvatarLink 分類の既存 Rotation/Parent 系コンストレイント（Unity + VRC）を削除
            // Internal 分類（義手内部参照）のものは保護して触らない
            foreach (var (component, kind) in ConstraintInspector.GetRotationConstraints(prostheticBone, prostheticRoot))
            {
                if (kind == ConstraintClassification.AvatarLink)
                    Undo.DestroyObjectImmediate(component);
            }
            foreach (var (component, kind) in ConstraintInspector.GetParentConstraints(prostheticBone, prostheticRoot))
            {
                if (kind == ConstraintClassification.AvatarLink)
                    Undo.DestroyObjectImmediate(component);
            }

            // VRC Rotation Constraint を追加
            var constraint = Undo.AddComponent<VRCRotationConstraint>(prostheticBone.gameObject);

            // ソースを設定
            constraint.Sources.Add(new VRCConstraintSource(avatarBone, 1.0f));

            // オフセット計算: 現在の姿勢差分を保持
            constraint.RotationAtRest = prostheticBone.localEulerAngles;
            Quaternion offset = Quaternion.Inverse(avatarBone.rotation) * prostheticBone.rotation;
            constraint.RotationOffset = offset.eulerAngles;

            // アクティブ化
            constraint.Locked = true;
            constraint.IsActive = true;

            EditorUtility.SetDirty(constraint);
        }
    }
}
