using IceMilkTea.Core;
using UnityEngine;

public partial class EnemyGrunt
{
    /// <summary>
    ///　攻撃れた時のステート
    /// </summary>
    private class EnemyGrunt_Damaged : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Enter()
        {
            Debug.Log("entered damaged state");
        }

        protected override void Update()
        {
            if (!Context.isDamaged)
            {
                Context.navAgent.enabled = true;
                Context.EvaluateCombatState();
            }            
        }

        protected override void Exit()
        {
            Debug.Log("exited damaged state");
        }

    }
}
