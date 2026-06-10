using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 速度バフ/デバフのロジックを担うシステム。
/// BuffManagerに登録され、SpeedBuffDataの管理とバフ適用APIを提供する。
/// </summary>
public class SpeedBuffSystem : IBuffSystem
{
    private readonly Dictionary<string, SpeedBuffData> dataMap = new Dictionary<string, SpeedBuffData>();

    /// <summary>ロード済みの全SpeedBuffDataを返す（Editor確認用）</summary>
    public IReadOnlyDictionary<string, SpeedBuffData> LoadedData => dataMap;
    private readonly IReadOnlyList<SpeedBuffReceiver> receivers;

    /// <param name="receivers">BuffManagerが管理するエンティティリスト</param>
    public SpeedBuffSystem(IReadOnlyList<SpeedBuffReceiver> receivers)
    {
        this.receivers = receivers;
    }

    // ─── IBuffSystem ────────────────────────────────────────────────

    public bool CanHandle(BuffData data) => data is SpeedBuffData;

    public void RegisterData(BuffData data)
    {
        if (data is not SpeedBuffData speedData)
        {
            return;
        }

        if (dataMap.ContainsKey(speedData.BuffId))
        {
            Debug.LogWarning($"[SpeedBuffSystem] BuffId '{speedData.BuffId}' が重複しています。後から登録されたものを無視します。");
            return;
        }

        dataMap[speedData.BuffId] = speedData;
    }

    public void Tick(float deltaTime)
    {
        foreach (var receiver in receivers)
        {
            receiver.GetContainer().Tick(deltaTime);
        }
    }

    // ─── バフ適用API ────────────────────────────────────────────────

    /// <summary>
    /// 指定エンティティに速度バフを適用する。
    /// </summary>
    public void Apply(SpeedBuffReceiver receiver, string buffId, float duration = -1f)
    {
        if (!TryGetData(buffId, out var data))
        {
            return;
        }

        receiver.GetContainer().AddBuff(data.CreateInstance(duration));
    }

    /// <summary>
    /// 対象種別に応じて速度バフを適用する。
    /// </summary>
    /// <param name="directTarget">target=Directの場合に指定する対象</param>
    public void Apply(SpeedBuffTarget target, string buffId, float duration = -1f, SpeedBuffReceiver directTarget = null)
    {
        if (!TryGetData(buffId, out var data))
        {
            return;
        }

        switch (target)
        {
            case SpeedBuffTarget.Direct:
                directTarget?.GetContainer().AddBuff(data.CreateInstance(duration));
                break;
            case SpeedBuffTarget.NonPlayer:
                ApplyNonPlayer(data, duration);
                break;
            case SpeedBuffTarget.World:
                ApplyWorld(data, duration);
                break;
        }
    }

    /// <summary>
    /// プレイヤー以外の全エンティティに速度バフを適用する。
    /// </summary>
    public void ApplyNonPlayer(string buffId, float duration = -1f)
    {
        if (!TryGetData(buffId, out var data))
        {
            return;
        }

        ApplyNonPlayer(data, duration);
    }

    /// <summary>
    /// 登録済みの全エンティティに速度バフを適用する（ワールド全体）。
    /// </summary>
    public void ApplyWorld(string buffId, float duration = -1f)
    {
        if (!TryGetData(buffId, out var data))
        {
            return;
        }

        ApplyWorld(data, duration);
    }

    // ─── バフ除去API ────────────────────────────────────────────────

    /// <summary>
    /// 指定エンティティから特定IDのバフを全件除去する。
    /// </summary>
    public void Remove(SpeedBuffReceiver receiver, string buffId)
    {
        receiver.GetContainer().RemoveBuff(buffId);
    }

    /// <summary>
    /// 指定エンティティの全速度バフを除去する。
    /// </summary>
    public void Clear(SpeedBuffReceiver receiver)
    {
        receiver.GetContainer().ClearBuffs();
    }

    // ─── 速度取得API ────────────────────────────────────────────────

    /// <summary>
    /// 指定エンティティの現在の速度倍率を取得する。
    /// </summary>
    public float GetMultiplier(SpeedBuffReceiver receiver) => receiver.SpeedMultiplier;

    // ─── 内部ヘルパー ───────────────────────────────────────────────

    private void ApplyNonPlayer(SpeedBuffData data, float duration)
    {
        foreach (var receiver in receivers)
        {
            if (!receiver.IsPlayer)
            {
                receiver.GetContainer().AddBuff(data.CreateInstance(duration));
            }
        }
    }

    private void ApplyWorld(SpeedBuffData data, float duration)
    {
        foreach (var receiver in receivers)
        {
            receiver.GetContainer().AddBuff(data.CreateInstance(duration));
        }
    }

    private bool TryGetData(string buffId, out SpeedBuffData data)
    {
        if (!dataMap.TryGetValue(buffId, out data))
        {
            Debug.LogWarning($"[SpeedBuffSystem] 存在しないバフIDが指定されました。buffId: {buffId}");
            return false;
        }

        return true;
    }
}
