using Cysharp.Threading.Tasks;

public interface IDebugModule
{
    /// <summary>
    /// モジュールの
    /// </summary>
    public string ModuleID { get; }

    /// <summary>
    /// モジュールの表示名
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// モジュールの説明
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 実行
    /// </summary>
    public UniTask Execute();
}