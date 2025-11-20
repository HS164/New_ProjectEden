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
            Context.navAgent.SetDestination(Context.playerPos);

            // プレイヤーが見えるまで近づく
            if (!Context.playerInSight)
            {
                return;
            }

            // 現在の戦闘状態の戦闘範囲内に到達したらステートを変更する
            if (Context.meleeState && Vector3.Distance(Context.transform.position, Context.playerPos) < Context.meleeCombatRange - Context.combatRangeHalfLength)
            {
				Context.UpdateState(EnemyState.MELEE);
                Debug.Log("switching to combat");
                return;
            }
            else if (!Context.meleeState && Vector3.Distance(Context.transform.position, Context.playerPos) < Context.rangedCombatRange - Context.combatRangeHalfLength)
            {
                Context.UpdateState(EnemyState.RANGED);
                Debug.Log("switching to combat");
                return;
            }

        }

        protected override void Exit()
        {
            Context.navAgent.isStopped = true;
        }
    }
}