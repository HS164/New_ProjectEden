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
            Context.navAgent.isStopped = true;
        }
    }
}
