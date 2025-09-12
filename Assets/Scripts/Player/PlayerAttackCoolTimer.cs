using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerAttackCoolTimer
{
    private float attackableCoolTime;
    // 攻撃可能か判定するフラグ
    private bool _nonAttackable = false;

    private bool isOverDrive = false;

    // 参照用プロパティ
    public bool nonAttackable { get { return _nonAttackable; } }

    // クールタイム解消用のコレクショントークン
    private CancellationTokenSource cts;

    // コンストラクタ
    // インスタンス生成時に攻撃のクールタイムの時間を設定する
    public PlayerAttackCoolTimer(float time)
    {
        attackableCoolTime = time;
    }

    public async void StartAttackCoolDown()
    {
        if (nonAttackable || isOverDrive)
        {
            // すでにクールタイムが始まっているとき、
            // もしくはクールタイムを行わないときに処理が走らないようにする
            return;
        }

        // 攻撃不可フラグをtrue;
        _nonAttackable = true;

        Debug.Log("start attack cool timer. time : " + attackableCoolTime);

        cts?.Dispose();
        cts = new CancellationTokenSource();

        try
        {
            // クールタイム分待機する
            await UniTask.Delay(TimeSpan.FromSeconds(attackableCoolTime), cancellationToken: cts.Token);

            _nonAttackable = false;
            Debug.Log("complete attack cool down");
        }
        catch(OperationCanceledException)
        {
            // キャンセルされた場合にはフラグを戻す
            _nonAttackable = false;
            Debug.Log("cancel cool down");
        }
        finally
        {
            // キャンセル用のコレクショントークンの初期化
            cts?.Dispose();
            cts = null;
        }
    }

    public void ChangeOverDrive(bool change)
    {
        if (nonAttackable)
        {
            // クールタイム中であればキャンセルしてクールタイムをリセット
            _nonAttackable = false;
            cts.Cancel();
        }
        Debug.Log("change to OverDrive State. change = " + change);

        isOverDrive = change;
    }

    public void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
