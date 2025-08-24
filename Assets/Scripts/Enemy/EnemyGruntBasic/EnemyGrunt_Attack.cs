using IceMilkTea.Core;
using UnityEngine;

public partial class EnemyGrunt
{
    /// <summary>
    /// 攻撃用ステート
    /// </summary>
    private class EnemyGrunt_Attack : ImtStateMachine<EnemyGrunt>.State
    {
        protected override void Update()
        {
            Context.CheckPlayerVisible();

            if (Context.isRangedAttacking)
            {
                Vector3 dir = Context.playerPos - Context.transform.position;
                dir.y = 0;
                if(dir.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(dir);
                    //Context.transform.rotation = lookRotation;
                    Context.transform.rotation = Quaternion.RotateTowards(Context.transform.rotation, lookRotation, Time.deltaTime * Context.navAgent.angularSpeed);
                }
            }
            
            if(!Context.isAttacking)
            {
                Context.stateMachine.SendEvent((int)StateTransition.COMBAT);
            }
        }
    }
}