using UnityEngine;

public class SelectableEnemyBase : MonoBehaviour , IPlayerSelectable
{
    protected IPlayerSelectable.SelectType selectType = IPlayerSelectable.SelectType.NONE;
    protected void Start()
    {
        selectType = IPlayerSelectable.SelectType.ENEMY;
    }

    // インターフェースメソッド
    // 選択可能かどうかを返すメソッド
    public bool IsSelected()
    {
        // 戻り値は動的に変えれるようにする？
        return true;
    }

    // インターフェースメソッド
    // 自分のタイプを返却
    public IPlayerSelectable.SelectType GetSelectType()
    {
        return selectType;
    }

    // インターフェースメソッド
    // 自分の幅、高さのサイズを取得
    public Vector3 GetBoundsSize()
    {
        var renderer = GetComponent<Renderer>();
        Vector3 boundsSize = transform.localScale;

        if (renderer != null)
        {
            boundsSize = renderer.bounds.size;
        }

        return boundsSize;
    }
}