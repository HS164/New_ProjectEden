using UnityEngine;

public class EnemyBase : MonoBehaviour , IPlayerSelectable
{
    private static readonly string TAG = "EnemyBase >> ";

    protected IPlayerSelectable.SelectType selectType = IPlayerSelectable.SelectType.NONE;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        Debug.Log(TAG + "Start()");
        selectType = IPlayerSelectable.SelectType.ENEMY;
    }

    // インターフェースメソッド
    // 選択可能かどうかを返すメソッド
    public bool IsSelected()
    {
        Debug.Log(TAG + "IsSelected()");
        // 戻り値は動的に変えれるようにする？
        return true;
    }

    // インターフェースメソッド
    // 自分のタイプを返却
    public IPlayerSelectable.SelectType GetSelectType()
    {
        Debug.Log(TAG + "GetSelectType(). selectType : " + selectType);
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

        Debug.Log(TAG + "boundsSize : " + boundsSize);
        return boundsSize;
    }
}
