using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using IceMilkTea.Core;
using UnityEngine;

/// <summary>
/// エネミーの基礎クラス
/// </summary>
public partial class EnemyBase : SelectableEnemyBase, IDamageable, IEnemy
{
    private ImtStateMachine<EnemyBase> stateMachine;

    // 基礎データ用のクラス
    [SerializeField] protected EnemyData data;

    // 管理用ブール
    protected bool isInitialized = false;
    protected bool isDead = false;
    protected bool isStunned = false;
    protected bool isHitStunned = false;
    protected bool isSelected = false;

    /// <summary>
    /// AIステートの移動ENUM
    /// </summary>
    enum EnemyState
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
        stateMachine.AddTransition<EnemyBase_Init, EnemyBase_Idle>((int)EnemyState.IDLE);
        stateMachine.AddTransition<EnemyBase_Patrol, EnemyBase_Idle>((int)EnemyState.IDLE);
        stateMachine.AddTransition<EnemyBase_Chase, EnemyBase_Idle>((int)EnemyState.IDLE);
        stateMachine.AddTransition<EnemyBase_Idle, EnemyBase_Patrol>((int)EnemyState.PATROL);
        stateMachine.AddTransition<EnemyBase_Chase, EnemyBase_Patrol>((int)EnemyState.PATROL);
        stateMachine.AddTransition<EnemyBase_Idle, EnemyBase_Chase>((int)EnemyState.CHASE);
        stateMachine.AddTransition<EnemyBase_Patrol, EnemyBase_Chase>((int)EnemyState.CHASE);

        stateMachine.SetStartState<EnemyBase_Init>();
    }

    /// <summary>
    /// 初期化用
    /// </summary>
    protected void Start()
    {
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
    public virtual bool Damage(float damageValue)
    {
        Debug.Log("enemy hit");
        return false;
    }

    public virtual void Death()
    {

    }

    public virtual EnemyType GetEnemyType()
    {
        throw new System.NotImplementedException();
    }

    public virtual bool IsMeleeAttacking()
    {
        throw new System.NotImplementedException();
    }

    public virtual bool IsMeleeState()
    {
        throw new System.NotImplementedException();
    }

    public virtual bool InCombat()
    {
        throw new System.NotImplementedException();
    }

    public virtual Transform GetTransform()
    {
        return transform;
    }
}
