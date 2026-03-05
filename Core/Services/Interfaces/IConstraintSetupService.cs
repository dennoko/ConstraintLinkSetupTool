using System.Collections.Generic;
using UnityEngine;
using Tiloop.ConstraintLinkSetupTool.Core.Models;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services.Interfaces
{
    public interface IConstraintSetupService
    {
        /// <summary>
        /// 提供されたボーンのペア（マップ）に対して、コンストレイントを実行します。
        /// 元の位置や回転を維持したまま同期する設定を構築します。
        /// </summary>
        void ApplyConstraints(List<BonePair> bonePairs, SetupConfig config);
        
        /// <summary>
        /// 設定されたコンストレイントを解除し、元の状態（スクリプト実行直前）に近づける
        /// </summary>
        void RemoveConstraints(List<BonePair> bonePairs, SetupConfig config);
    }
}
