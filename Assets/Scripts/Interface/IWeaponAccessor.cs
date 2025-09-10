using UnityEngine;

public interface IWeaponAccessor
{
    // 武器を装備
    void EquipWeapon(Transform parent);

    // 装備を解除
    void UnEquipWeapon();
}
