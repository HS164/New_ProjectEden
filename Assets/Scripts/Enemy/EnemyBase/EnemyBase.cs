using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using IceMilkTea.Core;
using UnityEngine;

/// <summary>
/// エネミーの基礎クラス
/// </summary>
public partial class EnemyBase : CharacterBase, IDamageable, IPlayerSelectable
{
    private ImtStateMachine<EnemyBase> stateMachine;

    // 基礎データ用のクラス
    protected EnemyData data;

    // 適当なブール
    protected bool isInitialized = false;
    protected bool isAlive = true;
    protected bool isStunned = false;
    protected bool isHitStunned = false;
    protected bool isPoisoned = false;
    protected bool isSelected = false;

    /// <summary>
    /// AIステートの移動ENUM
    /// </summary>
    enum StateTransition
    {
        IDLE,
        PATROL,
        CHASE,
        COMBAT,
        SEARCH,
    }

    /// <summary>
    /// ステートマシンのセットアップ
    /// </summary>
    private void Awake()
    {
        stateMachine = new ImtStateMachine<EnemyBase>(this);
        stateMachine.AddTransition<EnemyBase_Init, EnemyBase_Idle>((int)StateTransition.IDLE);
        stateMachine.AddTransition<EnemyBase_Patrol, EnemyBase_Idle>((int)StateTransition.IDLE);
        stateMachine.AddTransition<EnemyBase_Chase, EnemyBase_Idle>((int)StateTransition.IDLE);
        stateMachine.AddTransition<EnemyBase_Idle, EnemyBase_Patrol>((int)StateTransition.PATROL);
        stateMachine.AddTransition<EnemyBase_Chase, EnemyBase_Patrol>((int)StateTransition.PATROL);
        stateMachine.AddTransition<EnemyBase_Idle, EnemyBase_Chase>((int)StateTransition.CHASE);
        stateMachine.AddTransition<EnemyBase_Patrol, EnemyBase_Chase>((int)StateTransition.CHASE);

        stateMachine.SetStartState<EnemyBase_Init>();
    }

    /// <summary>
    /// 初期化用
    /// </summary>
    private void Start()
    {
        InitData();

        stateMachine.Update();
    }

    /// <summary>
    /// データの初期化
    /// </summary>
    protected virtual void InitData()
    {
        isInitialized = true;
    }

    /// <summary>
    /// アップデート関数、ステートマシン動かす
    /// </summary>
    protected virtual void FixedUpdate()
    {
        stateMachine.Update();
    }

    /// <summary>
    /// IDamageのダメージ関数、攻撃受けた時用
    /// </summary>
    /// <param name="damageValue"></param>
    public virtual void Damage(float damageValue)
    {
        // なんかダメージ計算
    }

    /// <summary>
    /// ISelectableの選択されているかの判定
    /// </summary>
    public virtual bool IsSelected()
    {
        return isSelected;
    }
}
