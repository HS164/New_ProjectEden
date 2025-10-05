using UnityEngine;

// プレイヤーの腕にアタッチするオブジェクト
public class WeaponManager : MonoBehaviour
{
    private IWeaponAccessor currentWeapon = null;

    public void ChangeWeapon(IWeaponAccessor newWeapon)
    {
        if(newWeapon == null)
        {
            return;
        }

        if(currentWeapon != null)
        {
            currentWeapon.UnEquipWeapon();
        }
        currentWeapon = newWeapon;
        currentWeapon.EquipWeapon(transform);
    }
}
