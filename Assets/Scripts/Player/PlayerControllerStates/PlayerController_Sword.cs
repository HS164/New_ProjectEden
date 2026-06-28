using IceMilkTea.Core;
using UnityEngine;

public partial class PlayerController
{
    /// <summary>
    /// 短剣用ステート
    /// </summary>
    private class PlayerController_Sword : ImtStateMachine<PlayerController>.State
    {
        protected override void Update()
        {
            if (Context.input.Attack.WasPressedThisFrame())
            {
                if (Context.currentWeapon == null)
                {
                    // 武器ワープ
                    Context.TelepotationTarget();
                    return;
                }

                if (Context.isTargetingWeapon)
                {
                    // 武器ワープ
                    Context.TelepotationTarget();
                    return;
                }
                else if (Context.isTargetingEnemy)
                {
                    // 敵ワープ
                    Context.TelepotationTarget();
                    return;
                }

                Context.BasicAttack();
            }
        }
    }
}