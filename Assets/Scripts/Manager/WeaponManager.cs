using UnityEngine;

// プレイヤーの腕にアタッチするオブジェクト
public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform rightHandPoint;
    [SerializeField] private Transform leftHandPoint;

    // 両手持ちのものが追加されたら両手持ち用を作る。
    private IWeaponAccessor rightHandWeapon;
    private IWeaponAccessor leftHandWeapon;

    public void ChangeWeapon(IWeaponAccessor newWeapon)
    {
        if (newWeapon == null)
        {
            return;
        }

        if (newWeapon is SelectableWeaponBase weapon)
        {
            Debug.Log("WeaponName : " + weapon.GetWeaponData().weaponName);
        }

        if (newWeapon.Slot == WeaponSlot.RightHand)
        {
            rightHandWeapon?.UnEquipWeapon();
            rightHandWeapon = newWeapon;
            rightHandWeapon.EquipWeapon(rightHandPoint);
        }
        else
        {
            leftHandWeapon?.UnEquipWeapon();
            leftHandWeapon = newWeapon;
            leftHandWeapon.EquipWeapon(leftHandPoint);
        }
    }
}
