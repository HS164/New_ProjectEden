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
            Context.navAgent.speed = Context.chaseSpeed;
            Context.navAgent.isStopped = false;
        }

        protected override void Update()
        {
            Context.CheckPlayerVisible();

            if(Context.isAttacking)
            {
                return;
            }

            if (Vector3.Distance(Context.transform.position, Context.playerPos) < Context.meleeAttackRange && !Context.isAttacking && Context.canAttack)
            {
                Context.MeleeAttack();
                Context.canAttack = false;
                Context.navAgent.isStopped = true;
                Context.stateMachine.SendEvent((int)StateTransition.ATTACK);
                Debug.Log("Attack start");
                return;
            }

            if (Vector3.Distance(Context.transform.position, Context.playerPos) < Context.rangedAttackRange && !Context.isAttacking && Context.canAttack)
            {
                Context.RangedAttack();
                Context.canAttack = false;
                Context.navAgent.isStopped = true;
                Context.stateMachine.SendEvent((int)StateTransition.ATTACK);
                Debug.Log("Attack start");
                return;
            }

            if (Vector3.Distance(Context.transform.position, Context.playerPos) > Context.chaseStartDistance)
            {
                Context.stateMachine.SendEvent((int)StateTransition.CHASE);
                Debug.Log("Switching to chase");
                return;
            }

            Context.navAgent.SetDestination(Context.playerPos);
        }

        protected override void Exit()
        {
            //Context.navAgent.isStopped = true;
        }
    }
}
