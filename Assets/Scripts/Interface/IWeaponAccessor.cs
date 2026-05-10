using UnityEngine;

public interface IWeaponAccessor
{
    WeaponSlot Slot { get; }

    // 武器を装備
    void EquipWeapon(Transform parent);

    // 装備を解除
    void UnEquipWeapon();

    void OnAttack();
    void OnHoldStart();
    void OnHoldEnd();

    // コンボによる段階ゲージによって変わる攻撃
    // 引数に段階ゲージを示すものを渡して処理することを想定
    void OnAttackComboState();
    // 奥義
    void OnSpecialMove();
}
