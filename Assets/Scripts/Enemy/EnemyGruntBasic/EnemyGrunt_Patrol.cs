using System.Linq;
using DocumentFormat.OpenXml.Math;
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

            if (Context.playerDetected)
            {
                Context.stateMachine.SendEvent((int)StateTransition.CHASE);
            }

            NavMeshAgent agent = Context.navAgent;
            if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if(!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                {
                    Vector3 currentPatrolPoint = Context.patrolPointList.First();
                    Context.patrolPointList.RemoveAt(0);
                    Context.patrolPointList.Add(currentPatrolPoint);
                    Context.stateMachine.SendEvent((int)StateTransition.IDLE);
                }
            }
        }

        protected override void Exit()
        {
            Context.navAgent.ResetPath();
            Context.navAgent.isStopped = true;
            Context.canPatrol = false;
            Context.ResetPatrol().Forget();
        }
    }
}