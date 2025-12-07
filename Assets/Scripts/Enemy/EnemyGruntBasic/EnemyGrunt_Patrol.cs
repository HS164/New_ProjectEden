using System.Linq;
using IceMilkTea.Core;
using UnityEngine;
using UnityEngine.AI;

public partial class EnemyGrunt
{
    /// <summary>
    /// パトロール用ステート
    /// </summary>
    private class EnemyGrunt_Patrol : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Enter()
        {
            Context.navAgent.speed = Context.patrolSpeed;
            Context.navAgent.isStopped = false;
            Context.navAgent.SetDestination(Context.patrolPointList.First());
        }

        protected override void Update()
        {
            Context.CheckPlayerVisible();

            NavMeshAgent agent = Context.navAgent;
            if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if(!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                {
                    Vector3 currentPatrolPoint = Context.patrolPointList.First();
                    Context.patrolPointList.RemoveAt(0);
                    Context.patrolPointList.Add(currentPatrolPoint);
					Context.UpdateState(EnemyState.IDLE);
				}
            }
        }

        protected override void Exit()
        {
            if (Context.navAgent.enabled)
            {
                Context.navAgent.ResetPath();
                Context.navAgent.isStopped = true;
            }
            Context.canPatrol = false;
            Context.ResetPatrol().Forget();
        }
    }
}