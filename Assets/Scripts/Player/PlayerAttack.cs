using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    PlayerController player;
    List<IDamageable> enemiesHit;

    void Start()
    {
        player = transform.parent.GetComponent<PlayerController>();
        enemiesHit = new List<IDamageable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.gameObject.GetComponent<IDamageable>();

        if (enemy != null && !enemiesHit.Contains(enemy))
        {
            enemiesHit.Add(enemy);
            var death = enemy.Damage(player.GetAttack(), transform.position);

            if (death)
            {
                var selectableObj = other.gameObject.GetComponent<IPlayerSelectable>();
                if (selectableObj != null)
                {
                    selectableObj.OnRelease();
                }
                enemy.Death();
            }
        }
    }

    public void ResetAttackCollider()
    {
        // ここでゲージとコンボの増加をする
        if (enemiesHit.Count > 0)
        {
            // 増加関数
            //ComboManager.Instance.AddWeaponCombo();
        }
        enemiesHit = new List<IDamageable>();
    }
}
