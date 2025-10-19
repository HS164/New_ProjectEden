using UnityEngine;

public abstract class SelectableEnemyBase : MonoBehaviour , IPlayerSelectable
{
    protected SelectableType selectType = SelectableType.NONE;

    protected void Start()
    {
        selectType = SelectableType.ENEMY;
    }

    /**
     * 選択候補になっているかどうか
     */
    public virtual void AllowSelection(bool selecttion)
    {

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
    public bool IsSelect { get; protected set; }

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
}
