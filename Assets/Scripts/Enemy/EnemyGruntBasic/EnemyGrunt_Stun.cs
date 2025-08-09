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
            // something to lower the timer, unless theres some other function that does that

            if (!Context.isStunned)
            {
                Context.stateMachine.SendEvent((int)StateTransition.IDLE);
            }
        }

        protected override void Exit()
        {

        }
    }
}
