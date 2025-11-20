using IceMilkTea.Core;
using UnityEngine;

public partial class EnemyGrunt
{
    /// <summary>
    /// 初期化用ステート
    /// </summary>
    private class EnemyGrunt_Init : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Enter()
        {
            Context.InitData();
            Context.Initialize();
        }

        protected override void Update()
        {
            if (Context.isInitialized)
            {
				Context.UpdateState(EnemyState.IDLE);
			}
        }

        protected override void Exit()
        {

        }
    }
}