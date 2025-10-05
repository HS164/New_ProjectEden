using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class SelectableWeaponBase : SelectableGimmickObjectBase , IWeaponAccessor
{
    [SerializeField]
    private WeaponData weaponData;

    // クールタイム解消用のコレクショントークン
    private CancellationTokenSource cts;

    [SerializeField]
    private float delayReleaseTime = 1;
    [SerializeField]
    private float destroyTime = 10;

    public WeaponData GetWeaponData()
    {
        return weaponData;
    }

    // 武器を装備
    public void EquipWeapon(Transform parent)
    {
        // ここで武器を装備する
        if (parent == null)
        {
            Debug.Log("parent is null. cant equip");
            return;
        }

        // 武器を親オブジェクトに装備させる
        transform.SetParent(parent);
        cts?.Cancel();
    }

    // 装備を解除
    public void UnEquipWeapon()
    {
        // 武器の装備を解除する
        transform.SetParent(null);
        DelayRelease().Forget();
        Destroy();
    }

    public override void OnRelease()
    {
        if (!IsSelect)
        {
            // すでに解除時の処理を行っていた場合即時終了
            return;
        }
    }

    private async UniTaskVoid Destroy()
    {
        cts?.Dispose();
        cts = new();

        var cancelFlag = false;

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(destroyTime), cancellationToken: cts.Token, ignoreTimeScale: true);
        }
        catch (OperationCanceledException)
        {
            cancelFlag = true;
        }
        finally
        {
            cts?.Dispose();
            cts = null;
        }
        if (!cancelFlag)
        {
            Destroy(gameObject);
        }
    }

    private async UniTaskVoid DelayRelease()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delayReleaseTime), ignoreTimeScale: true);
        IsSelect = false;
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}
