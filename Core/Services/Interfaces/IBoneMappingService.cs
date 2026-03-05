using System.Collections.Generic;
using UnityEngine;
using Tiloop.ConstraintLinkSetupTool.Core.Models;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services.Interfaces
{
    public interface IBoneMappingService
    {
        /// <summary>
        /// アバター内の指定されたボーンを起点として、パーツ（義手）側のボーンと自動的にマッチングする
        /// </summary>
        /// <param name="avatarBaseBone">追従させたいアバター側のベースボーン（例：右肩）</param>
        /// <param name="prostheticBaseBone">対応するパーツ側のベースボーン（例：義手の右肩部分）</param>
        /// <param name="sideMode">左右判定モード（Auto/Right/Left）</param>
        /// <returns>マッピングが完了した BonePair のリスト</returns>
        List<BonePair> MatchBones(Transform avatarBaseBone, Transform prostheticBaseBone, SideMode sideMode);
    }
}
