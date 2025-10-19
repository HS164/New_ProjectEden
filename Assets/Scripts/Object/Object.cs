using DocumentFormat.OpenXml.Office2013.Drawing.Chart;
using UnityEngine;
using static IPlayerSelectable;

public class Object : MonoBehaviour, IPlayerSelectable, IDamageable
{
    protected SelectableType selectType = SelectableType.NONE;
    protected float maxHP = 10f;
    protected float currentHP;

    protected void Start()
    {
        selectType = SelectableType.ENEMY;
        currentHP = maxHP;
    }

    /**
     * 選択候補になっているかどうか
     */
    public virtual void AllowSelection(bool selecttion)
    {
        transform.parent.GetComponent<MeshRenderer>().material.color = selecttion ? Color.red : new Color32(214, 156, 118, 255);
    }

    // インターフェースメソッド
    // 自分のタイプを返却
    public SelectableType GetSelectableType()
    {
        return selectType;
    }

    // インターフェースメソッド
    // 自分の幅、高さのサイズを取得
    public virtual Vector3 GetBoundsSize()
    {
        var renderer = GetComponent<Renderer>();
        Vector3 boundsSize = transform.localScale;

        if (renderer != null)
        {
            boundsSize = renderer.bounds.size;
        }

        return boundsSize;
    }

    // インターフェースメソッド
    // 選択中かどうかの判定
    public bool IsSelect { get; set; }

    // インターフェースメソッド
    // 選択時に行う処理
    public virtual void OnSelect()
    {
        if (IsSelect)
        {
            // すでに選択時の処理を行っていた場合即時終了
            return;
        }
        IsSelect = true;
    }

    // インターフェースメソッド
    // 選択を解除したときに行う処理
    public virtual void OnRelease()
    {
        if (!IsSelect)
        {
            // すでに解除時の処理を行っていた場合即時終了
            return;
        }
        IsSelect = false;
    }

    public bool Damage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            return true;
        }
        return false;
    }

    public void Death()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
