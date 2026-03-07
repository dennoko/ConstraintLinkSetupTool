using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services
{
    public enum ConstraintClassification
    {
        /// <summary>すべてのソースが義手サブツリー内 → 保護対象</summary>
        Internal,
        /// <summary>ソースなし、またはいずれかのソースが義手サブツリー外 → 置換対象</summary>
        AvatarLink,
    }

    /// <summary>
    /// ボーン上のコンストレイントを「内部参照」と「アバターリンク」に分類するユーティリティ
    /// </summary>
    public static class ConstraintInspector
    {
        /// <summary>
        /// transformがprostheticRoot以下に属するか（prostheticRoot自身を含む）
        /// </summary>
        private static bool IsInsideProsthetic(Transform transform, Transform prostheticRoot)
        {
            if (transform == null || prostheticRoot == null) return false;
            Transform t = transform;
            while (t != null)
            {
                if (t == prostheticRoot) return true;
                t = t.parent;
            }
            return false;
        }

        /// <summary>
        /// Unityコンストレイント（IConstraint）を分類する
        /// ソースが0件 or いずれかが義手外 → AvatarLink
        /// 全ソースが義手内 → Internal
        /// </summary>
        private static ConstraintClassification ClassifyUnity(IConstraint constraint, Transform prostheticRoot)
        {
            if (constraint.sourceCount == 0) return ConstraintClassification.AvatarLink;
            for (int i = 0; i < constraint.sourceCount; i++)
            {
                if (!IsInsideProsthetic(constraint.GetSource(i).sourceTransform, prostheticRoot))
                    return ConstraintClassification.AvatarLink;
            }
            return ConstraintClassification.Internal;
        }

        /// <summary>
        /// VRCコンストレイント（VRCConstraintBase）を分類する
        /// ソースが0件 or いずれかが義手外 → AvatarLink
        /// 全ソースが義手内 → Internal
        /// </summary>
        private static ConstraintClassification ClassifyVRC(VRCConstraintBase constraint, Transform prostheticRoot)
        {
            var sources = constraint.Sources;
            if (sources.Count == 0) return ConstraintClassification.AvatarLink;
            foreach (var src in sources)
            {
                if (!IsInsideProsthetic(src.SourceTransform, prostheticRoot))
                    return ConstraintClassification.AvatarLink;
            }
            return ConstraintClassification.Internal;
        }

        /// <summary>
        /// ボーン上のRotation系コンストレイント（Unity RotationConstraint + VRCRotationConstraint）を取得して分類する
        /// </summary>
        public static List<(Component component, ConstraintClassification kind)> GetRotationConstraints(
            Transform bone, Transform prostheticRoot)
        {
            var result = new List<(Component, ConstraintClassification)>();
            foreach (var c in bone.GetComponents<RotationConstraint>())
                result.Add((c, ClassifyUnity(c, prostheticRoot)));
            foreach (var c in bone.GetComponents<VRCRotationConstraint>())
                result.Add((c, ClassifyVRC(c, prostheticRoot)));
            return result;
        }

        /// <summary>
        /// ボーン上のParent系コンストレイント（Unity ParentConstraint + VRCParentConstraint）を取得して分類する
        /// </summary>
        public static List<(Component component, ConstraintClassification kind)> GetParentConstraints(
            Transform bone, Transform prostheticRoot)
        {
            var result = new List<(Component, ConstraintClassification)>();
            foreach (var c in bone.GetComponents<ParentConstraint>())
                result.Add((c, ClassifyUnity(c, prostheticRoot)));
            foreach (var c in bone.GetComponents<VRCParentConstraint>())
                result.Add((c, ClassifyVRC(c, prostheticRoot)));
            return result;
        }

        /// <summary>
        /// ボーン上にInternal分類のコンストレイントが1つでも存在するか
        /// </summary>
        public static bool HasInternalConstraints(Transform bone, Transform prostheticRoot)
        {
            foreach (var (_, kind) in GetRotationConstraints(bone, prostheticRoot))
                if (kind == ConstraintClassification.Internal) return true;
            foreach (var (_, kind) in GetParentConstraints(bone, prostheticRoot))
                if (kind == ConstraintClassification.Internal) return true;
            return false;
        }
    }
}
