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
            //if (Vector3.Distance(Context.transform.position, Context.playerLocation) < Context.combatStartDistance)
            //{
            //    Context.stateMachine.SendEvent((int)StateTransition.COMBAT);
            //    Debug.Log("switching to combat");
            //}

            if(!Context.playerDetected)
            {
                Context.stateMachine.SendEvent((int)StateTransition.IDLE);
                Debug.Log("追従停止、アイドルに移動");
            }

            Context.navAgent.SetDestination(Context.playerLocation);
        }

        protected override void Exit()
        {
            Context.navAgent.isStopped = true;
        }
    }
}