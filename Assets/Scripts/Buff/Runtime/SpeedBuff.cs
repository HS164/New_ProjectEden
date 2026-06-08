/// <summary>
/// 1つの速度バフ/デバフを表すデータクラス。
/// </summary>
public class SpeedBuff
{
    /// <summary>バフの種別ID。重複判定に使用する。</summary>
    public string BuffId { get; }

    /// <summary>速度への乗算値。1.0より大きければバフ、小さければデバフ。</summary>
    public float Multiplier { get; }

    /// <summary>同じBuffIdを複数重複して付与できるか。</summary>
    public bool IsStackable { get; }

    /// <summary>残り持続時間（秒）。負値の場合は永続扱い。</summary>
    public float Duration { get; private set; }

    public bool IsPermanent => Duration < 0f;
    public bool IsExpired => !IsPermanent && Duration <= 0f;

    /// <param name="buffId">バフ種別ID（重複判定キー）</param>
    /// <param name="multiplier">速度への乗算値</param>
    /// <param name="isStackable">同じIDで重複付与を許可するか</param>
    /// <param name="duration">持続時間（秒）。-1で永続。</param>
    public SpeedBuff(string buffId, float multiplier, bool isStackable, float duration = -1f)
    {
        BuffId = buffId;
        Multiplier = multiplier;
        IsStackable = isStackable;
        Duration = duration;
    }

    /// <summary>
    /// 経過時間を反映して残り持続時間を減らす。
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (!IsPermanent)
        {
            Duration -= deltaTime;
        }
    }
}
