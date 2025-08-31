using IceMilkTea.Core;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks.Triggers;
using Unity.VisualScripting;

public partial class EnemyGrunt : EnemyBase
{
    ImtStateMachine<EnemyGrunt> stateMachine;

    private NavMeshAgent navAgent;

    private GameObject player;
    [SerializeField] private GameObject attackBox;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletShootPoint;

    private CancellationTokenSource playerLoseDetectionCts;
    private CancellationTokenSource damageFlashCts;

    [SerializeField] private Vector3[] patrolPoints;

    [SerializeField] private List<Vector3> patrolPointList;

    private MeshRenderer mesh;
    private Color originalColor;

    private Vector3 playerPos;

    [SerializeField] private float chaseSpeed = 15;
    [SerializeField] private float patrolSpeed = 10;

    [SerializeField] private float patrolWaitTime = 5;
    [SerializeField] private float playerSearchTime = 5;
    [SerializeField] private float playerSearchAngle = 120;

    [SerializeField] private float meleeAttackTime = 0.8f;
    [SerializeField] private float rangedAttackDelay = 1.0f;
    [SerializeField] private float rangedAttackInterval = 0.2f;
    [SerializeField] private float meleeAttackCooldown = 1.25f;
    [SerializeField] private float rangedAttackCooldown = 0.75f;
    [SerializeField] private int rangedAttackCount = 3;

    [SerializeField] private float damageFlashTime = 0.1f;
    [SerializeField] private float deathFadeTime = 1f;

    [SerializeField] private float rangedAttackRange = 5f;
    [SerializeField] private float meleeAttackRange = 2.5f;
    [SerializeField] private float meleeStartRange = 1.5f;

    [SerializeField] private float playerSearchDistance;
    [SerializeField] private float playerChaseDistance;
    [SerializeField] private float combatStartDistance;
    [SerializeField] private float chaseStartDistance;

    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private int bulletDamage = 5;

    private bool playerDetected = false;
    private bool playerInSight = false;
    private bool canPatrol = false;
    private bool canAttack = false;
    private bool isAttacking = false;
    private bool isRangedAttacking = false;
    private bool isDamaged = false;

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
        DAMAGED,
        DEAD,
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
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Idle>((int)StateTransition.IDLE);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Patrol>((int)StateTransition.PATROL);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Chase>((int)StateTransition.CHASE);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Chase>((int)StateTransition.CHASE);
        stateMachine.AddTransition<EnemyGrunt_Combat, EnemyGrunt_Chase>((int)StateTransition.CHASE);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Chase>((int)StateTransition.CHASE);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Combat>((int)StateTransition.COMBAT);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Combat>((int)StateTransition.COMBAT);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Combat>((int)StateTransition.COMBAT);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Combat>((int)StateTransition.COMBAT);
        stateMachine.AddTransition<EnemyGrunt_Stun, EnemyGrunt_Combat>((int)StateTransition.COMBAT);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Combat>((int)StateTransition.COMBAT);

        stateMachine.AddTransition<EnemyGrunt_Combat, EnemyGrunt_Attack>((int)StateTransition.ATTACK);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Stun>((int)StateTransition.STUN);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Stun>((int)StateTransition.STUN);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Stun>((int)StateTransition.STUN);
        stateMachine.AddTransition<EnemyGrunt_Combat, EnemyGrunt_Stun>((int)StateTransition.STUN);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Stun>((int)StateTransition.STUN);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Stun>((int)StateTransition.STUN);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Damaged>((int)StateTransition.DAMAGED);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Damaged>((int)StateTransition.DAMAGED);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Damaged>((int)StateTransition.DAMAGED);
        stateMachine.AddTransition<EnemyGrunt_Combat, EnemyGrunt_Damaged>((int)StateTransition.DAMAGED);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Damaged>((int)StateTransition.DAMAGED);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Death>((int)StateTransition.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Death>((int)StateTransition.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Death>((int)StateTransition.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Combat, EnemyGrunt_Death>((int)StateTransition.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Death>((int)StateTransition.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Death>((int)StateTransition.DEAD);

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
        mesh = GetComponent<MeshRenderer>();
        originalColor = mesh.material.color;
        attackBox.SetActive(false);
        isInitialized = true;
        canPatrol = true;
        canAttack = true;
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
            playerPos = player.transform.position;
        }

        if (playerVisible && !playerInSight)
        {
            playerLoseDetectionCts?.Cancel();
            playerLoseDetectionCts?.Dispose();
            playerLoseDetectionCts = null;

            playerDetected = true;
            playerInSight = true;
        }
        else if (!playerVisible && playerInSight)
        {
            playerLoseDetectionCts?.Cancel();
            playerLoseDetectionCts?.Dispose();

            playerInSight = false;
            playerLoseDetectionCts = new CancellationTokenSource();
            ResetPlayerDetection(playerLoseDetectionCts.Token).Forget();
        }
    }

    /// <summary>
    /// IDamageのダメージ関数、攻撃受けた時用
    /// </summary>
    /// <param name="damageValue"></param>
    public override bool Damage(float damageValue)
    {
        if(isDead)
        {
            return isDead;
        }

        data.HP -= (int)damageValue;
        if(data.HP > 0)
        {
            isDamaged = true;
            stateMachine.SendEvent((int)StateTransition.DAMAGED);

            damageFlashCts?.Cancel();
            damageFlashCts?.Dispose();
            damageFlashCts = new CancellationTokenSource();
            DamageFlash(damageFlashCts.Token).Forget();

            Debug.Log($"Enemy [{gameObject.name}] took {damageValue} damage. HP {data.HP}/{data.maxHP}");
        }
        else
        {
            isDead = true;
            stateMachine.SendEvent((int)StateTransition.DEAD);
        }
        return isDead;
    }

    public override void Death()
    {
        DeathMotion().Forget();
        Debug.Log($"Enemy [{gameObject.name}] died");
    }

    private void MeleeAttack()
    {
        MeleeAttackAnimation().Forget();
    }

    private void RangedAttack()
    {
        RangedAttackAnimation().Forget();
    }

    private void ShootProjectile()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletShootPoint.position, Quaternion.identity);

        IProjectile projectile = bullet.GetComponent<IProjectile>();

        projectile.Fire(transform.forward, bulletSpeed, bulletDamage);
    }

    private async UniTaskVoid DamageFlash(CancellationToken token)
    {
        try
        {
            mesh.material.color = Color.red;

            float elapsed = 0f;
            while (elapsed < damageFlashTime)
            {
                elapsed += Time.deltaTime;
                mesh.material.color = Color.Lerp(Color.red, originalColor, elapsed / damageFlashTime);
                await UniTask.Yield(cancellationToken: token);
            }

            isDamaged = false;

            if (playerDetected)
            {
                stateMachine.SendEvent((int)StateTransition.COMBAT);
            }
            else
            {
                stateMachine.SendEvent((int)StateTransition.IDLE);
            }
        }
        finally
        {
            mesh.material.color = originalColor;
        }
    }

    private async UniTaskVoid DeathMotion()
    {
        mesh.material.color = Color.magenta;

        float elapsed = 0f;
        while (elapsed < deathFadeTime)
        {
            elapsed += Time.deltaTime;
            mesh.material.color = Color.Lerp(Color.magenta, Color.clear, elapsed / deathFadeTime);
            await UniTask.Yield();
        }

        Destroy(gameObject);
    }

    private async UniTaskVoid MeleeAttackAnimation()
    {
        isAttacking = true;
        attackBox.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(meleeAttackTime));
        attackBox.SetActive(false);
        isAttacking = false;
        ResetAttackCooldown(meleeAttackCooldown).Forget();
    }

    private async UniTaskVoid RangedAttackAnimation()
    {
        isAttacking = true;
        isRangedAttacking = true;
        navAgent.updateRotation = false;
        await UniTask.Delay(TimeSpan.FromSeconds(rangedAttackDelay));
        for (int i = 0; i < rangedAttackCount; i++)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(rangedAttackInterval));
            ShootProjectile();
        }
        isRangedAttacking = false;
        navAgent.updateRotation = true;
        await UniTask.Delay(TimeSpan.FromSeconds(rangedAttackDelay));
        isAttacking = false;
        ResetAttackCooldown(rangedAttackCooldown).Forget();
    }

    private async UniTaskVoid ResetPatrol()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(patrolWaitTime));
        canPatrol = true;
    }

    private async UniTaskVoid ResetAttackCooldown(float attackCooldown)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(attackCooldown));
        canAttack = true;
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
