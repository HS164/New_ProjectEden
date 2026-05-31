using UnityEngine;

public class SpearWeapon : SelectableWeaponBase
{
    // エイム中かどうかを表す状態。アニメーションや UI への通知に使う
    private bool isAiming = false;

    protected override void OnAttackCombo(int comboStep)
    {
        if (comboStep == 0)
        {
            // 突き
        }
        else if (comboStep == 1)
        {
            // 横薙ぎ
        }
    }

    // 右クリック長押し開始時
    public override void OnHoldStart()
    {
        isAiming = true;
        // エイム開始
    }

    // 右クリック長押し解除時
    public override void OnHoldEnd()
    {
        isAiming = false;
        // エイム解除 → 槍投げ
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
