using UnityEngine;

public class TempAttackCollider : MonoBehaviour
{
    [SerializeField] float attackDamage = 15f;

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            bool targetDead = false;
            targetDead = damageable.Damage(attackDamage, transform.position);
            if (targetDead)
            {
                damageable.Death();
            }
        }
    }
}
