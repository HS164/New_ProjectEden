using IceMilkTea.Core;
using UnityEngine;

public partial class EnemyGrunt
{
    /// <summary>
    /// 追従用ステート
    /// </summary>
    private class EnemyGrunt_Chase : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Enter()
        {
            Context.navAgent.speed = Context.chaseSpeed;
            Context.navAgent.isStopped = false;
        }

        protected override void Update()
        {
            Context.CheckPlayerVisible();

            // まだ戦闘関連未実装
            if (Vector3.Distance(Context.transform.position, Context.playerPos) < Context.combatStartDistance)
            {
                Context.stateMachine.SendEvent((int)StateTransition.COMBAT);
                Debug.Log("switching to combat");
                return;
            }

            if (!Context.playerDetected)
            {
                Context.stateMachine.SendEvent((int)StateTransition.IDLE);
                Debug.Log("追従停止、アイドルに移動");
                return;
            }

            Context.navAgent.SetDestination(Context.playerPos);
        }

        protected override void Exit()
        {
            Context.navAgent.isStopped = true;
        }
    }
}