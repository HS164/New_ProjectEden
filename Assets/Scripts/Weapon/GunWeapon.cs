using UnityEngine;

public class GunWeapon : SelectableWeaponBase
{
    // エイム中かどうかを表す状態。アニメーションや UI への通知に使う
    private bool _isAiming = false;

    protected override void OnAttackCombo(int comboStep)
    {
        if (comboStep == 0)
        {
            // 射撃
        }
    }

    // 右クリック長押し開始時
    public override void OnHoldStart()
    {
        _isAiming = true;
        // エイム開始
    }

    // 右クリック長押し解除時
    public override void OnHoldEnd()
    {
        _isAiming = false;
        // エイム解除
    }

    // コンボによる段階ゲージによって変わる攻撃
    public override void OnAttackComboState()
    {
        // プレイヤー側で実装？
    }
    // 奥義
    public override void OnSpecialMove()
    {
        // プレイヤー側で実装？
    }
}
