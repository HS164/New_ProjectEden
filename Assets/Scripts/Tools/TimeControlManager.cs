using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class TimeControl
{
    public static async UniTask KronoDelay(float seconds)
    {
        float elapsed = 0;
        while (elapsed < seconds)
        {
            await UniTask.WaitUntil(() => !PlayerKronoEnd.GetIsKronoEnd());
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }
    }

    public static async UniTask KronoDelay(float seconds, CancellationToken token)
    {
        float elapsed = 0;
        while (elapsed < seconds)
        {
            await UniTask.WaitUntil(() => !PlayerKronoEnd.GetIsKronoEnd(), cancellationToken: token);
            elapsed += Time.deltaTime;
            await UniTask.Yield(cancellationToken: token);
        }
    }

    public static async UniTask KronoYield()
    {
        await UniTask.WaitUntil(() => !PlayerKronoEnd.GetIsKronoEnd());
        await UniTask.Yield();
    }

    public static async UniTask KronoYield(CancellationToken token)
    {
        await UniTask.WaitUntil(() => !PlayerKronoEnd.GetIsKronoEnd(), cancellationToken: token);
        await UniTask.Yield(cancellationToken: token);
    }
}