using IceMilkTea.Core;
using UnityEngine;

public partial class EnemyGrunt
{
    /// <summary>
    /// エネミーのスタン・のけぞりステート
    /// </summary>
    private class EnemyGrunt_Stun : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Enter()
        {

        }

        protected override void Update()
        {
            if (!Context.isStunned)
            {
                Context.stateMachine.SendEvent((int)StateTransition.IDLE);
                return;
            }
        }

        protected override void Exit()
        {

        }
    }
}
