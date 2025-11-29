using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerAttackCoolTimer
{
    private float attackableCoolTime;
    // 効果時間
    private float overDriveTime;
    // 攻撃可能か判定するフラグ
    private bool _nonAttackable = false;

    private bool isOverDrive = false;

    // 参照用プロパティ
    public bool nonAttackable { get { return _nonAttackable; } }

    // クールタイム解消用のコレクショントークン
    private CancellationTokenSource attackCoolDownCts;

    // コンストラクタ
    // インスタンス生成時に攻撃のクールタイムの時間を設定する
    public PlayerAttackCoolTimer(float attackableCoolTime, float overDriveTime)
    {
        this.attackableCoolTime = attackableCoolTime;
        this.overDriveTime = overDriveTime;
    }

    public async void StartAttackCoolDown()
    {
        if (nonAttackable || isOverDrive)
        {
            // すでにクールタイムが始まっているとき、
            // もしくはクールタイムを行わないときに処理が走らないようにする
            return;
        }

        // 攻撃不可フラグをtrueにする
        _nonAttackable = true;
        // 視覚的にわかりやすくするため色を変える
        Color originalColor = Color.white;
        if (player != null)
        {
            originalColor = player.transform.GetComponent<MeshRenderer>().sharedMaterial.color;
            player.transform.GetComponent<MeshRenderer>().sharedMaterial.color = Color.yellow;
        }

        Debug.Log("start attack cool timer. time : " + attackableCoolTime);

        attackCoolDownCts?.Dispose();
        attackCoolDownCts = new CancellationTokenSource();

        try
        { 
            // クールタイム分待機する
            await UniTask.Delay(TimeSpan.FromSeconds(attackableCoolTime), cancellationToken: attackCoolDownCts.Token, ignoreTimeScale: true);

            _nonAttackable = false;
            Debug.Log("complete attack cool down");
            if(player != null)
            {
                // 色をもとに戻す
                player.transform.GetComponent<MeshRenderer>().sharedMaterial.color = originalColor;
            }
        }
        catch(OperationCanceledException)
        {
            // キャンセルされた場合にはフラグを戻す
            _nonAttackable = false;
            Debug.Log("cancel cool down");
            if (player != null)
            {
                // 色をもとに戻す
                player.transform.GetComponent<MeshRenderer>().sharedMaterial.color = originalColor;
            }
        }
        finally
        {
            // キャンセル用のコレクショントークンの初期化
            attackCoolDownCts?.Dispose();
            attackCoolDownCts = null;
        }
    }

    // クールタイム処理のキャンセル
    public void CancelCoolDown()
    {
        if (!nonAttackable)
        {
            // タイマー実行中でない場合即座に処理を終了
            return;
        }

        Debug.Log("cancel AttackCoolDown");
        // キャンセルを受け付けた場合、キャンセルを行う。
        _nonAttackable = false;
        attackCoolDownCts.Cancel();
    }

    public void ChangeOverDrive()
    {
        if (nonAttackable)
        {
            // クールタイム中であればキャンセルしてクールタイムをリセット
            _nonAttackable = false;
            attackCoolDownCts.Cancel();
        }
        Debug.Log("change to OverDrive State");

        StartOverDrive().Forget();
    }
    
    // OverDriveの効果時間
    private async UniTaskVoid StartOverDrive()
    {
        if (isOverDrive)
        {
            // 既に実行中の場合即座に終了
            return;
        }
        isOverDrive = true;
        Debug.Log("Start OverDrive");

        await UniTask.Delay(TimeSpan.FromSeconds(overDriveTime), ignoreTimeScale: true);
        
        isOverDrive = false;
    }

    public void OnDestroy()
    {
        // 念のため初期化処理
        attackCoolDownCts?.Cancel();
        attackCoolDownCts?.Dispose();
    }

    // 攻撃のクールタイム中かどうかわかりやすくするためのコード
    // -----------------------ここから-----------------------
    private Transform player;

    public void SetPlayer(Transform player)
    {
        this.player = player;
    }
    // -----------------------ここまで-----------------------
}
