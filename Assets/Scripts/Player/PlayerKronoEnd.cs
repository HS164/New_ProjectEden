using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerKronoEnd
{
    // 動きを止める時間
    private float timeToStopTime;

    // 現在クロノ・エンドを行っているかの判定
    // TimeScaleがstaticなため、staticにし全体で共通の判定を持つ必要がある
    private static bool isKronoEnd = false;

    // クールタイム解消用のコレクショントークン
    private CancellationTokenSource cts;

    public PlayerKronoEnd(float timeToStopTime)
    {
        this.timeToStopTime = timeToStopTime;
    }

    public static bool GetIsKronoEnd()
    {
        return isKronoEnd;
    }

    public async UniTask<bool> StartKronoEnd()
    {
        // スキルが実行されたかどうかの判定
        bool result = false;

        if (isKronoEnd)
        {
            // すでにクールタイムが始まっているとき、
            // もしくはクールタイムを行わないときに処理が走らないようにする
            return result;
        }

        Debug.Log("start KronoEnd");
        // スキル中判定をtrueにする
        isKronoEnd = true;
        Time.timeScale = 0.01f;

        cts?.Dispose();
        cts = new CancellationTokenSource();

        try
        {
            // クールタイム分待機する（TimeScaleの影響を受けないようにする）
            await UniTask.Delay(TimeSpan.FromSeconds(timeToStopTime), cancellationToken: cts.Token, ignoreTimeScale: true);
            ResetTimeScale();
            result = true;
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合にはフラグを戻す
            ResetTimeScale();
            result = true;
        }
        finally
        {
            // キャンセル用のコレクショントークンの初期化
            cts?.Dispose();
            cts = null;
        }
        return result;
    }

    public void CancelKronoEnd()
    {
        if (!isKronoEnd)
        {
            // スキル使用中でない場合即座に終了
            return;
        }

        Debug.Log("cancel KronoEnd");
        // スキルのキャンセルを受け付けた場合、キャンセルを行う。
        isKronoEnd = false;
        cts.Cancel();
    }

    // 判定とTimeScaleをリセットする
    private void ResetTimeScale()
    {
        isKronoEnd = false;
        Time.timeScale = 1.0f;
    }

    public void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
