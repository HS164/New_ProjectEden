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

            if (Context.playerDetected)
            {
                Context.stateMachine.SendEvent((int)StateTransition.CHASE);
                return;
            }

            if(Context.canPatrol)
            {
                Context.stateMachine.SendEvent((int)StateTransition.PATROL);
                return;
            }
        }

        protected override void Exit()
        {

        }
    }
}
