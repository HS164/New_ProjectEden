using IceMilkTea.Core;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public partial class EnemyGrunt
{
    /// <summary>
    /// エネミーの戦闘ステート
    /// </summary>
    private class EnemyGrunt_Melee : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Enter()
        {
            Context.navAgent.speed = Context.combatSpeed;
            Context.navAgent.isStopped = false;
        }

        protected override void Update()
        {
            Context.CheckPlayerVisible();

            if (Vector3.Distance(Context.transform.position, Context.playerPos) > Context.meleeCombatRange)
            {
                Context.UpdateState(EnemyState.CHASE);
                Debug.Log("Switching to chase");
                return;
            }

            if (!Context.isStrafing && !Context.canAttack)
            {
                Context.CombatMovement();
                return;
            }
            else if (Context.isStrafing)
            {
                NavMeshAgent agent = Context.navAgent;
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                    {
                        Context.isStrafing = false;
                        Context.ResetStrafeCooldown(1f).Forget();
                    }
                }
                return;
            }

            if (Context.canAttack)
            {
                Context.MeleeAttack();
                Context.canAttack = false;
                Context.navAgent.isStopped = false;
                Context.UpdateState(EnemyState.ATTACK);
                Debug.Log("Attack start");
                return;
            }
        }

        protected override void Exit()
        {
            Context.isStrafing = false;
            Context.ResetStrafeCooldown(1f).Forget();
        }
    }
}
