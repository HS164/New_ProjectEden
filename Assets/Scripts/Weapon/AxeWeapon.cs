using UnityEngine;

public class AxeWeapon : SelectableWeaponBase
{
    // ガード中に攻撃を受けたとき OnGuardSuccess() 経由で true になる
    private bool isCounterReady = false;
    protected override void OnAttackCombo(int comboStep)
    {
        // 連続攻撃で挙動が変わるようであれば以下のように使う
        if (comboStep == 0)
        {
            // 薙ぎ払い
        }
        else if (comboStep == 1)
        {
            // 叩き
        }
    }

    // 右クリック長押し開始時
    public override void OnHoldStart()
    {
        isCounterReady = false;
        // ガード開始
    }

    // 右クリック長押し解除時
    public override void OnHoldEnd()
    {
        if (isCounterReady)
        {
            // カウンター薙ぎ払い
            isCounterReady = false;
        }
        else
        {
            // ガード解除
        }
    }

    // 攻撃判定側からガード成功を通知する。OnHoldEnd 時にカウンター攻撃に切り替わる
    public void OnGuardSuccess()
    {
        isCounterReady = true;
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
