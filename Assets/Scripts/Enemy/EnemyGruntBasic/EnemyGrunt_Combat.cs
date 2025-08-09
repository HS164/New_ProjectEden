using IceMilkTea.Core;
using UnityEngine;

public partial class EnemyGrunt
{
    /// <summary>
    /// エネミーの戦闘ステート
    /// </summary>
    private class EnemyGrunt_Combat : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Enter()
        {
            Context.navAgent.isStopped = false;
        }

        protected override void Update()
        {
            Context.CheckPlayerVisible();

            if (Vector3.Distance(Context.transform.position, Context.playerLocation) > Context.chaseStartDistance)
            {
                Context.stateMachine.SendEvent((int)StateTransition.COMBAT);
                Debug.Log("switching to chase");
            }
        }

        protected override void Exit()
        {

        }
    }
}
