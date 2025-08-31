using UnityEngine;

public interface IPlayerSelectable
{
    // 選択先のタイプ
    enum SelectType
    {
        NONE, // あるだけ
        GIMMICK, // 取得可能オブジェクト
        ENEMY // 敵
    }

    // 選択候補かどうかの判定
    void RegisterelSectionCandidate(bool selecttion);
    // 選択タイプを取得するためのメソッド
    SelectType GetSelectType();
    // オブジェクトの幅を取得するためのメソッド
    Vector3 GetBoundsSize();
    // 選択したかどうかの判定
    bool IsSelect { get; set; }
    // 選択時に呼び出すメソッド
    void OnSelect();
    // 選択解除時に呼び出すメソッド
    void OnRelease();
}