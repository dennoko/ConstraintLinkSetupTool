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
        public Transform ManualAvatarBaseBone; // 任意指定（未指定時は自動解決）
        public Transform TargetProstheticBaseBone; // (例：義手側の右肩にあたる部分)

        public bool UseManualAvatarBaseBone = false;

        /// <summary>
        /// 実際に使用するアバター側ベースボーン
        /// - 手動指定が有効かつ設定済み: 手動値
        /// - それ以外: 義手側ベースボーンの親を自動使用
        /// </summary>
        public Transform TargetAvatarBaseBone
        {
            get
            {
                if (UseManualAvatarBaseBone && ManualAvatarBaseBone != null)
                {
                    return ManualAvatarBaseBone;
                }

                return AutoDetectedAvatarBaseBone;
            }
        }

        /// <summary>
        /// 自動検出されたアバター側ベースボーン（義手側ベースボーンの親）
        /// </summary>
        public Transform AutoDetectedAvatarBaseBone
        {
            get { return TargetProstheticBaseBone != null ? TargetProstheticBaseBone.parent : null; }
        }

        public SideMode PartSideMode = SideMode.Auto;

        /// <summary>
        /// アバター側のベースボーンからルートを自動取得
        /// </summary>
        public Transform TargetAvatarRoot
        {
            get { return TargetAvatarBaseBone != null ? TargetAvatarBaseBone.root : null; }
        }

        /// <summary>
        /// 義手側のルートを自動取得
        /// 義手がすでにアバターの子として配置されている場合は、
        /// 義手のベースボーンとアバターのベースボーンの共通祖先を検出し、
        /// 義手側の独立したルートを返す
        /// </summary>
        public Transform TargetProstheticRoot
        {
            get
            {
                if (TargetProstheticBaseBone == null) return null;
                
                // アバターのルートと異なれば、そのまま .root を返す（別のオブジェクト）
                Transform pRoot = TargetProstheticBaseBone.root;
                Transform aRoot = TargetAvatarRoot;
                
                if (aRoot == null || pRoot != aRoot)
                {
                    return pRoot;
                }
                
                // 義手がアバターの子として配置されている場合:
                // 義手のベースボーンから上に辿り、アバターのベースボーンの階層に含まれない
                // 最も上位のTransformを義手のルートとする
                return FindProstheticRoot(TargetProstheticBaseBone, TargetAvatarBaseBone);
            }
        }

        /// <summary>
        /// 義手のベースボーンから上に辿り、アバター側の階層から分岐する点を見つける
        /// </summary>
        private Transform FindProstheticRoot(Transform prostheticBone, Transform avatarBone)
        {
            // アバター側のベースボーンからルートまでの全祖先を収集
            var avatarAncestors = new HashSet<Transform>();
            Transform current = avatarBone;
            while (current != null)
            {
                avatarAncestors.Add(current);
                current = current.parent;
            }

            // 義手のベースボーンから上に辿り、アバターの祖先に含まれる親に到達したら
            // その一つ前（含まれていない最上位）が義手の独立ルート
            Transform prostheticRoot = prostheticBone;
            current = prostheticBone;
            while (current.parent != null)
            {
                if (avatarAncestors.Contains(current.parent))
                {
                    // current.parentはアバター側の階層なので、currentが義手の独立ルート
                    return current;
                }
                current = current.parent;
                prostheticRoot = current;
            }
            
            // 見つからなかった場合（通常ありえないが）、ベースボーン自体を返す
            return prostheticRoot;
        }

        public bool IsValid()
        {
            return TargetAvatarRoot != null && TargetProstheticRoot != null && TargetAvatarBaseBone != null && TargetProstheticBaseBone != null;
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
