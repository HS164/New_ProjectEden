using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 1エンティティ分の速度バフ/デバフを管理し、最終速度倍率を計算するクラス。
/// 基本速度 1.0 に対して全バフを乗算し、エンティティごとの速度制限でClampする。
/// </summary>
public class SpeedBuffContainer
{
    private const float BASE_SPEED = 1.0f;

    private readonly List<SpeedBuff> _buffs = new List<SpeedBuff>();
    private readonly float _minSpeedLimit;
    private readonly float _maxSpeedLimit;

    /// <summary>現在の最終速度倍率（速度制限適用済み）</summary>
    public float FinalMultiplier { get; private set; } = BASE_SPEED;

    /// <summary>速度倍率が変化した際の通知 (引数: 最終速度倍率)</summary>
    public event UnityAction<float> OnSpeedChanged;

    /// <param name="minSpeedLimit">速度倍率の下限（エンティティごとに設定）</param>
    /// <param name="maxSpeedLimit">速度倍率の上限（エンティティごとに設定）</param>
    public SpeedBuffContainer(float minSpeedLimit, float maxSpeedLimit)
    {
        _minSpeedLimit = minSpeedLimit;
        _maxSpeedLimit = maxSpeedLimit;
    }

    /// <summary>
    /// バフを追加する。
    /// IsStackable=false の場合、同じBuffIdが既に存在すれば追加しない。
    /// </summary>
    public void AddBuff(SpeedBuff buff)
    {
        if (!buff.IsStackable && _buffs.Exists(b => b.BuffId == buff.BuffId))
        {
            return;
        }

        _buffs.Add(buff);
        Recalculate();
    }

    /// <summary>
    /// 指定IDのバフを全件除去する。
    /// </summary>
    public void RemoveBuff(string buffId)
    {
        var removed = _buffs.RemoveAll(b => b.BuffId == buffId);
        if (removed > 0)
        {
            Recalculate();
        }
    }

    /// <summary>
    /// 全バフを除去して速度を基本値に戻す。
    /// </summary>
    public void ClearBuffs()
    {
        _buffs.Clear();
        Recalculate();
    }

    /// <summary>
    /// 毎フレーム呼び出す。期限切れバフを自動除去する。
    /// </summary>
    public void Tick(float deltaTime)
    {
        foreach (var buff in _buffs)
        {
            buff.Tick(deltaTime);
        }

        var beforeCount = _buffs.Count;
        _buffs.RemoveAll(b => b.IsExpired);

        if (_buffs.Count != beforeCount)
        {
            Recalculate();
        }
    }

    /// <summary>
    /// 全バフを乗算して最終速度倍率を求め、速度制限を適用する。
    /// </summary>
    private void Recalculate()
    {
        var result = BASE_SPEED;
        foreach (var buff in _buffs)
        {
            result *= buff.Multiplier;
        }

        var clamped = Mathf.Clamp(result, _minSpeedLimit, _maxSpeedLimit);
        if (Mathf.Approximately(FinalMultiplier, clamped))
        {
            return;
        }

        FinalMultiplier = clamped;
        OnSpeedChanged?.Invoke(FinalMultiplier);
    }
}
