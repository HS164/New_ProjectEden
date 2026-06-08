using UnityEngine;

/// <summary>
/// 速度バフ/デバフの定義データ。
/// Addressablesに登録し、ラベル "Buff" を付けることでBuffManagerが自動ロードする。
/// </summary>
[CreateAssetMenu(fileName = "NewSpeedBuff", menuName = "ProjectEden/Buff/SpeedBuffData")]
public class SpeedBuffData : BuffData
{
    [Tooltip("速度への乗算値。1.0より大きければバフ、小さければデバフ。")]
    [SerializeField] private float _multiplier = 1.0f;

    [Tooltip("同じIDのバフを重複して付与できるか。")]
    [SerializeField] private bool _isStackable = true;

    public float Multiplier => _multiplier;
    public bool IsStackable => _isStackable;

    /// <summary>
    /// この定義を元に持続時間付きのSpeedBuffインスタンスを生成する。
    /// </summary>
    /// <param name="duration">持続時間（秒）。-1で永続。</param>
    public SpeedBuff CreateInstance(float duration = -1f)
    {
        return new SpeedBuff(BuffId, _multiplier, _isStackable, duration);
    }
}
