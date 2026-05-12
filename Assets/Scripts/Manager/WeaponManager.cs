using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform rightHandPoint;
    [SerializeField] private Transform leftHandPoint;
    [SerializeField] private Transform bothHandsPoint;

    // 装備中の装備
    private IWeaponAccessor currentWeapon;

    public void ChangeWeapon(IWeaponAccessor newWeapon)
    {
        if (newWeapon == null)
        {
            return;
        }

        if (newWeapon is SelectableWeaponBase weapon)
        {
            Debug.Log("WeaponName : " + weapon.GetWeaponData().weaponName + ", WeaponSlot : " + newWeapon.Slot);
        }

        // 古い装備を解除
        currentWeapon?.UnEquipWeapon();
        // 新しい装備をセット
        currentWeapon = newWeapon;

        switch (newWeapon.Slot)
        {
            case WeaponSlot.RightHand:
                if (rightHandPoint != null)
                {
                    currentWeapon.EquipWeapon(rightHandPoint);
                }
                break;

            case WeaponSlot.LeftHand:
                if (leftHandPoint != null)
                {
                    currentWeapon.EquipWeapon(leftHandPoint);
                }
                break;

            case WeaponSlot.BothHands:
                if (bothHandsPoint != null)
                {
                    currentWeapon.EquipWeapon(bothHandsPoint);
                }
                break;
        }
    }
}
