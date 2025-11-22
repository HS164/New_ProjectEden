using IceMilkTea.Core;
using UnityEngine;

public partial class EnemyGrunt
{
    /// <summary>
    /// エネミーの待機ステート
    /// </summary>
    private class EnemyGrunt_Idle : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Enter()
        {

        }

        protected override void Update()
        {
            Context.CheckPlayerVisible();

            if(Context.canPatrol)
            {
                Context.UpdateState(EnemyState.PATROL);
                return;
            }
        }

        protected override void Exit()
        {

        }
    }
}
