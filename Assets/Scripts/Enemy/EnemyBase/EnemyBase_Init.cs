using IceMilkTea.Core;
using UnityEngine;

public partial class EnemyBase
{
    /// <summary>
    /// 初期化用ステート
    /// </summary>
    private class EnemyBase_Init : ImtStateMachine<EnemyBase>.State
    {
        protected override void Enter()
        {
            Context.InitData();
        }

        protected override void Update()
        {
            if (Context.isInitialized)
            {
                Context.stateMachine.SendEvent((int)StateTransition.IDLE);
            }
        }

        protected override void Exit()
        {

        }
    }
}