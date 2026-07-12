using IceMilkTea.Core;
using UnityEngine;

public partial class PlayerController
{
    /// <summary>
    /// 短剣用ステート
    /// </summary>
    private class PlayerController_Unarmed : ImtStateMachine<PlayerController>.State
    {
        protected override void Update()
        {

            if (Context.input.Attack.WasPressedThisFrame())
            {
                if (Context.currentWeapon == null)
                {
                    // weapon warp check
                    Context.TelepotationTarget();
                    return;
                }
            }
        }
    }
}