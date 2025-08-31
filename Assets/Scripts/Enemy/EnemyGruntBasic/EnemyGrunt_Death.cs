using IceMilkTea.Core;
using UnityEngine;

public partial class EnemyGrunt
{
    /// <summary>
    ///　死亡ステート
    /// </summary>
    private class EnemyGrunt_Death : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Enter()
        {
            Context.navAgent.isStopped = true;
        }
    }
}
