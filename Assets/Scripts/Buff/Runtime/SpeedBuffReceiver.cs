using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 速度バフ/デバフを受け取るエンティティにアタッチするコンポーネント。
/// Awake時にSpeedBuffManagerへ自動登録・OnDestroy時に自動解除される。
/// </summary>
public class SpeedBuffReceiver : MonoBehaviour
{
    [SerializeField] private float _minSpeedLimit = 0.1f;
    [SerializeField] private float _maxSpeedLimit = 3.0f;

    /// <summary>
    /// プレイヤー判定。NonPlayer対象のバフを受けなくなる。
    /// </summary>
    [SerializeField] private bool _isPlayer = false;

    public bool IsPlayer => _isPlayer;

    /// <summary>現在の最終速度倍率 (1.0 = 等速)</summary>
    public float SpeedMultiplier => _container?.FinalMultiplier ?? 1f;

    /// <summary>速度倍率が変化した際の通知 (引数: 最終速度倍率)</summary>
    public event UnityAction<float> OnSpeedChanged;

    private SpeedBuffContainer _container;

    private void Awake()
    {
        _container = new SpeedBuffContainer(_minSpeedLimit, _maxSpeedLimit);
        _container.OnSpeedChanged += mult => OnSpeedChanged?.Invoke(mult);
        BuffManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        BuffManager.Instance?.Unregister(this);
    }

    /// <summary>
    /// SpeedBuffManagerが内部的にバフコンテナへアクセスするために使用する。
    /// </summary>
    internal SpeedBuffContainer GetContainer() => _container;
}
