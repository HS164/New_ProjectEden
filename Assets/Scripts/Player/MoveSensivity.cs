using UnityEngine;

/// <summary>
/// プレイヤーの移動速度パラメータとスピードリンクのボーナス管理を行うクラス
/// </summary>
[System.Serializable]
public class MoveSensitivity
{
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float accelerationTime = 0.15f;  // 最高速度到達時間（秒）
    [SerializeField] private float decelerationTime = 0.1f;   // 完全停止時間（秒）
    [SerializeField] private float reversalFriction = 3f;     // 反転入力時の加速倍率
    [SerializeField] private float turnSpeed = 600f;          // 回転速度（度/秒）

    [SerializeField] private float increaseValue;
    [SerializeField, ReadOnly] private float speedBonus;

    public float MaxSpeed => maxSpeed + speedBonus;
    public float AccelerationRate => MaxSpeed / Mathf.Max(accelerationTime, 0.001f);
    public float DecelerationRate => MaxSpeed / Mathf.Max(decelerationTime, 0.001f);
    public float ReversalFriction => reversalFriction;
    public float TurnSpeed => turnSpeed;

    /// <summary>
    /// 初期化。スピードボーナスをリセットする
    /// </summary>
    public void Init() => speedBonus = 0;

    /// <summary>
    /// スピードリンク発動時にボーナス速度を加算する
    /// </summary>
    public void SpeedUp()
    {
        // スピードボーナスの上限を指定（上限の指定だけでよいためMinを使用）
        speedBonus = Mathf.Min(speedBonus + increaseValue, maxSpeed);
    }

    /// <summary>
    /// スピードボーナスをリセットする
    /// </summary>
    public void Reset() => speedBonus = 0;

    public float IncreaseValue { set => increaseValue = value; }
}
