/// <summary>
/// 速度バフ/デバフの対象種別。
/// </summary>
public enum SpeedBuffTarget
{
    /// <summary>
    /// 呼び出し元が対象を直接指定する（単体・範囲選択は呼び出し側で行う）。
    /// </summary>
    Direct,

    /// <summary>
    /// プレイヤー以外の全登録エンティティに適用する。
    /// プレイヤーのスキルによるスロー演出などに使用する。
    /// </summary>
    NonPlayer,

    /// <summary>
    /// 登録済みの全エンティティに適用する（危険予知・演出・奥義など）。
    /// </summary>
    World,
}
