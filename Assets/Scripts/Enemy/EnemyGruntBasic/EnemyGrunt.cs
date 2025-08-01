using IceMilkTea.Core;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class EnemyGrunt : EnemyBase
{
    ImtStateMachine<EnemyGrunt> stateMachine;

    private NavMeshAgent navAgent;

    private GameObject player;

    private CancellationTokenSource playerLoseDetectionCts;

    [SerializeField] private Vector3[] patrolPoints;

    [SerializeField] private List<Vector3> patrolPointList;

    private Vector3 playerLocation;
    private Vector2 rangedAttackrange;

    [SerializeField] private float chaseSpeed = 15;
    [SerializeField] private float patrolSpeed = 10;

    [SerializeField] private float patrolWaitTime = 5;
    [SerializeField] private float playerSearchTime = 5;
    [SerializeField] private float playerSearchAngle = 120;

    [SerializeField] private float meleeAttackRange;
    [SerializeField] private float meleeStartRange;

    [SerializeField] private float playerSearchDistance;
    [SerializeField] private float playerChaseDistance;
    [SerializeField] private float combatStartDistance;
    [SerializeField] private float chaseStartDistance;

    private bool playerDetected = false;
    private bool playerInSight = false;
    private bool canPatrol = false;

    /// <summary>
    /// AIステートの移動ENUM
    /// </summary>
    enum StateTransition
    {
        IDLE,
        PATROL,
        CHASE,
        COMBAT,
        ATTACK,
        STUN,
    }

    /// <summary>
    /// ステートマシンのセットアップ
    /// </summary>
    private void Awake()
    {
        stateMachine = new ImtStateMachine<EnemyGrunt>(this);

        stateMachine.AddTransition<EnemyGrunt_Init, EnemyGrunt_Idle>((int)StateTransition.IDLE);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Idle>((int)StateTransition.IDLE);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Idle>((int)StateTransition.IDLE);
        stateMachine.AddTransition<EnemyGrunt_Combat, EnemyGrunt_Idle>((int)StateTransition.IDLE);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Patrol>((int)StateTransition.PATROL);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Chase>((int)StateTransition.CHASE);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Chase>((int)StateTransition.CHASE);
        stateMachine.AddTransition<EnemyGrunt_Combat, EnemyGrunt_Chase>((int)StateTransition.CHASE);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Combat>((int)StateTransition.COMBAT);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Combat>((int)StateTransition.COMBAT);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Combat>((int)StateTransition.COMBAT);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Combat>((int)StateTransition.COMBAT);
        stateMachine.AddTransition<EnemyGrunt_Stun, EnemyGrunt_Combat>((int)StateTransition.COMBAT);

        stateMachine.AddTransition<EnemyGrunt_Combat, EnemyGrunt_Attack>((int)StateTransition.ATTACK);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Stun>((int)StateTransition.STUN);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Stun>((int)StateTransition.STUN);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Stun>((int)StateTransition.STUN);
        stateMachine.AddTransition<EnemyGrunt_Combat, EnemyGrunt_Stun>((int)StateTransition.STUN);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Stun>((int)StateTransition.STUN);

        stateMachine.SetStartState<EnemyGrunt_Init>();
    }

    /// <summary>
    /// データの初期化
    /// </summary>
    private new void Start()
    {
        InitData();

        stateMachine.Update();
    }

    /// <summary>
    /// アップデート関数、ステートマシン動かす
    /// </summary>
    protected new virtual void FixedUpdate()
    {
        stateMachine.Update();
    }

    private void Initialize()
    {
        player = GameObject.FindWithTag("Player");
        navAgent = GetComponent<NavMeshAgent>();
        isInitialized = true;
        canPatrol = true;
    }

    private void CheckPlayerVisible()
    {
        Vector3 dirToPlayer = player.transform.position - transform.position;

        if (!playerInSight)
        {
            // 頭を振り向く機能とかあれば頭を基準にしたい
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

            if (angleToPlayer > playerSearchAngle / 2f)
            {
                return;
            }
        }

        bool playerVisible = !Physics.Linecast(transform.position, player.transform.position, 0) && dirToPlayer.magnitude < playerSearchDistance;

        // プレイヤーの最新のポジションを取得
        if (playerVisible)
        {
            playerLocation = player.transform.position;
        }

        if (playerVisible && !playerInSight)
        {
            playerLoseDetectionCts?.Cancel();
            playerLoseDetectionCts?.Dispose();
            playerLoseDetectionCts = null;

            playerDetected = true;
            playerInSight = true;
        }
        else if(!playerVisible && playerInSight)
        {
            playerLoseDetectionCts?.Cancel();
            playerLoseDetectionCts?.Dispose();

            playerInSight = false;
            playerLoseDetectionCts = new CancellationTokenSource();
            ResetPlayerDetection(playerLoseDetectionCts.Token).Forget();
        }
    }

    private async UniTaskVoid ResetPatrol()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(patrolWaitTime));
        canPatrol = true;
    }

    private async UniTaskVoid ResetPlayerDetection(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(playerSearchTime), cancellationToken: token);
            playerDetected = false;
            playerInSight = false;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("正常に中断");
        }
    }
}
