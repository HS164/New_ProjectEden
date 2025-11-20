
using UnityEngine;

public interface IEnemy
{

    /// <summary>
    /// 敵の種類取得
    /// </summary>
    /// <returns></returns>
    EnemyType GetEnemyType();

    /// <summary>
    /// 現在近距離攻撃を行っているか取得する
    /// </summary>
    /// <returns></returns>
    bool IsMeleeAttacking();

    /// <summary>
    /// 現在近距離攻撃態勢か取得する
    /// </summary>
    /// <returns></returns>
    bool IsMeleeState();

    /// <summary>
    /// 現在戦闘中か取得する
    /// </summary>
    /// <returns></returns>
    bool InCombat();

    /// <summary>
    /// 敵のTransformを取得する
    /// </summary>
    /// <returns></returns>
    Transform GetTransform();

}

/// <summary>
/// 敵の種類
/// </summary>
public enum EnemyType
{
    NONE, // エラー用
    MELEE, // 近距離型
    RANGED, // 遠距離型
    HYBRID, // 近距離、遠距離可能
    TURRET, // タレット、固定された敵
    SPECIAL, // 特殊タイプ　（未定や特別枠）
    BOSS // ボス
}