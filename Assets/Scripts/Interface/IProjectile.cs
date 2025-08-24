using UnityEngine;

public interface IProjectile
{
    void Fire(Vector3 shootDir, float speed, int damage);
}