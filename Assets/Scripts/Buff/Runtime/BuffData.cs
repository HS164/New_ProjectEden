using UnityEngine;

/// <summary>
/// 全バフ種別共通の基底ScriptableObject。
/// 新しいバフ種別を追加する場合はこのクラスを継承する。
/// </summary>
public abstract class BuffData : ScriptableObject
{
    [Tooltip("バフの一意なID。コードから呼び出す際に使用する。")]
    [SerializeField] private string _buffId;

    [Tooltip("このバフの用途・使用場所などの説明。コードからは参照しない。")]
    [TextArea(2, 4)]
    [SerializeField] private string _description;

    public string BuffId => _buffId;
}
