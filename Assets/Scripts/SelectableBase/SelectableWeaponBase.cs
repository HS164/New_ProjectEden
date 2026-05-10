using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class SelectableWeaponBase : SelectableGimmickObjectBase , IWeaponAccessor
{
    [SerializeField]
    private WeaponData weaponData;

    [SerializeField]
    private WeaponSlot weaponSlot = WeaponSlot.RightHand;

    public WeaponSlot Slot => weaponSlot;

    // UnEquipWeapon の Destroy タイマー用
    private CancellationTokenSource cts;

    [SerializeField]
    private Vector3 holdPositionOffset = Vector3.zero;
    [SerializeField]
    private Vector3 holdRotationOffset = Vector3.zero;

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
        transform.SetLocalPositionAndRotation(holdPositionOffset, Quaternion.Euler(holdRotationOffset));
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

    // PlayerController からの呼び出し口。コンボ進行を内部で管理して OnAttackCombo に委譲する
    public void OnAttack()
    {
        // コンボ数を取得してきて、2段階目,3段階目で挙動を変えられるように
        // int comboStep = コンボ取得メソッド
        // OnAttackCombo(comboStep);
    }

    // 各武器クラスでオーバーライドして comboStep ごとの攻撃を実装する
    protected virtual void OnAttackCombo(int comboStep) { }

    // 各武器クラスでオーバーライドして右クリック長押し開始時の動作を実装する(エイム時とか)
    public virtual void OnHoldStart() { }

    // 各武器クラスでオーバーライドして右クリック長押し解除時の動作を実装する(カウンターとか槍投げとか)
    public virtual void OnHoldEnd() { }

    // コンボによる段階ゲージによって変わる攻撃
    public virtual void OnAttackComboState() { }
    // 奥義
    public virtual void OnSpecialMove() { }

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
