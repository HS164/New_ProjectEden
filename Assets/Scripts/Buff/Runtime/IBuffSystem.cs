/// <summary>
/// バフ種別ごとのシステムが実装するインターフェース。
/// 新しいバフ種別を追加する場合はこのインターフェースを実装し、BuffManagerに登録する。
/// </summary>
public interface IBuffSystem
{
    /// <summary>このシステムが対象のBuffDataを扱えるか判定する。</summary>
    bool CanHandle(BuffData data);

    /// <summary>BuffManagerがロードしたBuffDataを登録する。</summary>
    void RegisterData(BuffData data);

    /// <summary>毎フレーム呼び出される。期限切れバフの処理などに使用する。</summary>
    void Tick(float deltaTime);
}
