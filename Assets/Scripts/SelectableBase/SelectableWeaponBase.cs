using UnityEngine;

public class SelectableWeaponBase : SelectableGimmickObjectBase , IWeaponAccessor
{
    [SerializeField]
    public WeaponData weaponData;

    public WeaponData GetWeaponData()
    {
        return weaponData;
    }

    // 武器を装備
    public void EquipWeapon(Transform parent)
    {
        // ここで武器を装備する
        if (parent == null)
        {
            Debug.Log("parent is null. cant equip");
            return;
        }

        // 武器を親オブジェクトに装備させる
        transform.SetParent(parent);
    }

    // 装備を解除
    public void UnEquipWeapon()
    {
        // 武器の装備を解除する
        transform.SetParent(null);
    }
}
