using System.Numerics;

public interface IDamageable
{
    /// <summary>
    /// ダメージを処理する 死んだ場合にtrueを返す
    /// </summary>
    /// <param name="damageValue"></param>
    /// <returns></returns>
    bool Damage(float damageValue, UnityEngine.Vector3 hitPosition);

    void Death();
}