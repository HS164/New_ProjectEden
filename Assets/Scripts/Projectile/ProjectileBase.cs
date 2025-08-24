using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class ProjectileBase : MonoBehaviour, IProjectile
{
    private Rigidbody rbody;

    private int bulletDamage = 1;

    private CancellationTokenSource cts;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        rbody.isKinematic = false;
        rbody.useGravity = false;
        rbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        cts = new CancellationTokenSource();
        DestroyAfterTime(cts.Token).Forget();
    }

    private async UniTaskVoid DestroyAfterTime(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(10.0f), cancellationToken: token);
            Destroy(gameObject);
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if(damageable != null)
        {
            damageable.Damage(bulletDamage);
            cts.Cancel();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Iprojectile の弾発射関数、発射用の情報とダメージを設定する
    /// </summary>
    /// <param name="shootDir"></param>
    /// <param name="speed"></param>
    /// <param name="damage"></param>
    public virtual void Fire(Vector3 shootDir, float speed, int damage)
    {
        bulletDamage = damage;
        rbody.linearVelocity = shootDir * speed;
    }
}
