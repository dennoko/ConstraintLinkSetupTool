using System.Collections.Generic;
using UnityEngine;

namespace Tiloop.ConstraintLinkSetupTool.Core.Models
{
    public enum SideMode
    {
        Auto,
        Right,
        Left
    }

    /// <summary>
    /// セットアップに使用するコンフィギュア情報
    /// </summary>
    public class SetupConfig
    {
        public Transform AvatarRoot;
        public Transform ProstheticRoot;
        public Transform TargetAvatarBaseBone; // (例：アバター側の右肩)
        public Transform TargetProstheticBaseBone; // (例：義手側の右肩にあたる部分)

        public SideMode PartSideMode = SideMode.Auto;
        public bool EnablePositionConstraint = true;

        public bool IsValid()
        {
            return AvatarRoot != null && ProstheticRoot != null && TargetAvatarBaseBone != null && TargetProstheticBaseBone != null;
        }

        public void UpdateRootsFromBaseBones()
        {
            if (TargetAvatarBaseBone != null && AvatarRoot == null)
            {
                AvatarRoot = TargetAvatarBaseBone.root;
            }
            if (TargetProstheticBaseBone != null && ProstheticRoot == null)
            {
                ProstheticRoot = TargetProstheticBaseBone.root;
            }
        }
    }

    /// <summary>
    /// アバターボーンと義手ボーンのペアリング情報
    /// </summary>
    public class BonePair
    {
        public Transform AvatarBone { get; set; }
        public Transform ProstheticBone { get; set; }
        public bool IsBaseBone { get; set; }
        public bool ApplyConstraint { get; set; } = true;

        public BonePair(Transform avatarBone, Transform prostheticBone, bool isBaseBone = false)
        {
            AvatarBone = avatarBone;
            ProstheticBone = prostheticBone;
            IsBaseBone = isBaseBone;
        }
    }
}
