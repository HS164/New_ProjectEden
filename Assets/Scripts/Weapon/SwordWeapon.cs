using UnityEngine;

public class SwordWeapon : SelectableWeaponBase
{
    protected override void OnAttackCombo(int step)
    {
        // 連続攻撃で挙動が変わるようであれば以下のように使う
        if (step == 0)
        {
            // 縦斬り
        }
        else if (step == 1)
        {
            // 横斬り
        }
        else if (step == 2)
        {
            // 突き
        }
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
