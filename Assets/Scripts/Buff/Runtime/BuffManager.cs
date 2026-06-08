using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// バフシステム全体を統括する中央マネージャー。
/// 起動時にAddressablesからラベル "Buff" の全バフ定義を自動ロードし、
/// 対応するIBuffSystemへ振り分ける。
///
/// 新しいバフ種別を追加する手順:
///   1. BuffDataを継承したScriptableObjectを作成する
///   2. IBuffSystemを実装したシステムクラスを作成する
///   3. Awake内の InitializeSystems() に新システムを追加する
/// </summary>
public class BuffManager : SingletonBehaviour<BuffManager>
{
    private const string BUFF_LABEL = "Buff";

    private readonly List<SpeedBuffReceiver> _receivers = new List<SpeedBuffReceiver>();
    private readonly List<IBuffSystem> _systems = new List<IBuffSystem>();

    private SpeedBuffReceiver _playerReceiver = null;
    private AsyncOperationHandle<IList<BuffData>> _loadHandle;

    /// <summary>全バフ定義のロードが完了しているか</summary>
    public bool IsReady { get; private set; } = false;

    /// <summary>バフ定義のロード完了時に発行されるイベント</summary>
    public event UnityAction OnReady;

    /// <summary>速度バフシステム</summary>
    public SpeedBuffSystem Speed { get; private set; }

    // 新しいバフ種別を追加した場合はここに追加する
    // public AttackBuffSystem Attack { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InitializeSystems();
        LoadAllBuffDataAsync().Forget();
    }

    /// <summary>
    /// 使用するバフシステムを初期化・登録する。
    /// 新しいバフ種別を追加する場合はここに追加する。
    /// </summary>
    private void InitializeSystems()
    {
        Speed = new SpeedBuffSystem(_receivers);
        _systems.Add(Speed);

        // 新しいバフ種別を追加する場合はここに追加する
        // Attack = new AttackBuffSystem(_receivers);
        // _systems.Add(Attack);
    }

    private void Update()
    {
        foreach (var system in _systems)
        {
            system.Tick(Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        if (_loadHandle.IsValid())
        {
            Addressables.Release(_loadHandle);
        }
    }

    // ─── Addressablesロード ──────────────────────────────────────────

    /// <summary>
    /// ラベル "Buff" の全BuffDataをAddressablesから非同期ロードし、
    /// 各IBuffSystemへ振り分けて登録する。
    /// </summary>
    private async UniTaskVoid LoadAllBuffDataAsync()
    {
        _loadHandle = Addressables.LoadAssetsAsync<BuffData>(BUFF_LABEL, null);
        await _loadHandle.ToUniTask();

        if (_loadHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[BuffManager] バフ定義のロードに失敗しました。ラベル: {BUFF_LABEL}");
            return;
        }

        foreach (var data in _loadHandle.Result)
        {
            var handled = false;
            foreach (var system in _systems)
            {
                if (system.CanHandle(data))
                {
                    system.RegisterData(data);
                    handled = true;
                    break;
                }
            }

            if (!handled)
            {
                Debug.LogWarning($"[BuffManager] '{data.name}' を処理できるシステムが見つかりませんでした。対応するIBuffSystemを登録してください。");
            }
        }

        IsReady = true;
        OnReady?.Invoke();
    }

    // ─── エンティティ登録管理 ────────────────────────────────────────

    /// <summary>
    /// SpeedBuffReceiverをシステムへ登録する。SpeedBuffReceiverのAwakeから自動的に呼ばれる。
    /// </summary>
    public void Register(SpeedBuffReceiver receiver)
    {
        if (_receivers.Contains(receiver))
        {
            return;
        }

        _receivers.Add(receiver);

        if (receiver.IsPlayer)
        {
            _playerReceiver = receiver;
        }
    }

    /// <summary>
    /// SpeedBuffReceiverの登録を解除する。OnDestroyから自動的に呼ばれる。
    /// </summary>
    public void Unregister(SpeedBuffReceiver receiver)
    {
        _receivers.Remove(receiver);

        if (_playerReceiver == receiver)
        {
            _playerReceiver = null;
        }
    }

    // ─── 共通取得API ────────────────────────────────────────────────

    /// <summary>
    /// プレイヤーの速度倍率を取得する。未登録の場合は 1.0 を返す。
    /// </summary>
    public float GetPlayerSpeedMultiplier() => _playerReceiver?.SpeedMultiplier ?? 1f;
}
