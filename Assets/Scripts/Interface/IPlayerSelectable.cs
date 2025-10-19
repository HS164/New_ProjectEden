using UnityEngine;

public interface IPlayerSelectable
{
    // 選択候補かどうかの判定
    void AllowSelection(bool selecttion);
    // 選択タイプを取得するためのメソッド
    SelectableType GetSelectableType();
    // オブジェクトの幅を取得するためのメソッド
    Vector3 GetBoundsSize();
    // 選択したかどうかの判定
    bool IsSelect { get; }
    // 選択時に呼び出すメソッド
    void OnSelect();
    // 選択解除時に呼び出すメソッド
    void OnRelease();
}