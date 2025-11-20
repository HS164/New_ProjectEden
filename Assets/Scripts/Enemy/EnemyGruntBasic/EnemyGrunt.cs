using IceMilkTea.Core;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks.Triggers;
using Unity.VisualScripting;

struct KronoEndGruntData
{
    public bool wasStopped;
}

public partial class EnemyGrunt : EnemyBase
{
    ImtStateMachine<EnemyGrunt> stateMachine;

    private NavMeshAgent navAgent;

    private GameObject player;
    [SerializeField] private GameObject attackBox;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletShootPoint;

    private AudioSource audioSource;
    [SerializeField] private AudioClip gunClip;
    [SerializeField] private AudioClip swordClip;

    private CancellationTokenSource playerLoseDetectionCts;
    private CancellationTokenSource damageFlashCts;

    [SerializeField] private Vector3[] patrolPoints;

    [SerializeField] private List<Vector3> patrolPointList;

    private MeshRenderer mesh;
    private Color originalColor;

    private Vector3 playerPos;

    private KronoEndGruntData kronoEndData;

    private EnemyState currentState;
    [SerializeField] private EnemyType enemyType;

    [SerializeField] private float chaseSpeed = 15;
    [SerializeField] private float combatSpeed = 10;
    [SerializeField] private float patrolSpeed = 8;

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

    [SerializeField] private float meleeAttackRange = 2.5f;
    [SerializeField] private float meleeStartRange = 1.5f;


    [SerializeField] private float playerSearchDistance = 20f;
    [SerializeField] private float combatDetectionDistance = 30f;
    [SerializeField] private float meleeCombatRange = 10f;
    [SerializeField] private float rangedCombatRange = 20f;
    private float combatRangeHalfLength = 5f;

    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private int bulletDamage = 5;

    private bool playerDetected = false;
    private bool playerInSight = false;
    private bool meleeState = false;
    private bool canPatrol = false;
    private bool canAttack = false;
    private bool isAttacking = false;
    private bool isRangedAttacking = false;
    private bool isStrafing = false;
    private bool canStrafe = false;
    private bool isDamaged = false;
    private bool isFrozen = false;

    /// <summary>
    /// AIステートの移動ENUM
    /// </summary>
    enum EnemyState
    {
        IDLE,
        PATROL,
        CHASE,
        MELEE,
        RANGED,
        ATTACK,
        STUN,
        DAMAGED,
        DEAD
    }

    /// <summary>
    /// ステートマシンのセットアップ
    /// </summary>
    private void Awake()
    {
        stateMachine = new ImtStateMachine<EnemyGrunt>(this);

        stateMachine.AddTransition<EnemyGrunt_Init, EnemyGrunt_Idle>((int)EnemyState.IDLE);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Idle>((int)EnemyState.IDLE);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Idle>((int)EnemyState.IDLE);
        stateMachine.AddTransition<EnemyGrunt_Melee, EnemyGrunt_Idle>((int)EnemyState.IDLE);
        stateMachine.AddTransition<EnemyGrunt_Ranged, EnemyGrunt_Idle>((int)EnemyState.IDLE);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Idle>((int)EnemyState.IDLE);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Patrol>((int)EnemyState.PATROL);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Chase>((int)EnemyState.CHASE);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Chase>((int)EnemyState.CHASE);
        stateMachine.AddTransition<EnemyGrunt_Melee, EnemyGrunt_Chase>((int)EnemyState.CHASE);
        stateMachine.AddTransition<EnemyGrunt_Ranged, EnemyGrunt_Chase>((int)EnemyState.CHASE);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Chase>((int)EnemyState.CHASE);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Melee>((int)EnemyState.MELEE);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Melee>((int)EnemyState.MELEE);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Melee>((int)EnemyState.MELEE);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Melee>((int)EnemyState.MELEE);
        stateMachine.AddTransition<EnemyGrunt_Stun, EnemyGrunt_Melee>((int)EnemyState.MELEE);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Melee>((int)EnemyState.MELEE);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Ranged>((int)EnemyState.RANGED);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Ranged>((int)EnemyState.RANGED);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Ranged>((int)EnemyState.RANGED);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Ranged>((int)EnemyState.RANGED);
        stateMachine.AddTransition<EnemyGrunt_Stun, EnemyGrunt_Ranged>((int)EnemyState.RANGED);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Ranged>((int)EnemyState.RANGED);

        stateMachine.AddTransition<EnemyGrunt_Melee, EnemyGrunt_Attack>((int)EnemyState.ATTACK);
        stateMachine.AddTransition<EnemyGrunt_Ranged, EnemyGrunt_Attack>((int)EnemyState.ATTACK);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Stun>((int)EnemyState.STUN);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Stun>((int)EnemyState.STUN);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Stun>((int)EnemyState.STUN);
        stateMachine.AddTransition<EnemyGrunt_Melee, EnemyGrunt_Stun>((int)EnemyState.STUN);
        stateMachine.AddTransition<EnemyGrunt_Ranged, EnemyGrunt_Stun>((int)EnemyState.STUN);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Stun>((int)EnemyState.STUN);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Stun>((int)EnemyState.STUN);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Damaged>((int)EnemyState.DAMAGED);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Damaged>((int)EnemyState.DAMAGED);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Damaged>((int)EnemyState.DAMAGED);
        stateMachine.AddTransition<EnemyGrunt_Melee, EnemyGrunt_Damaged>((int)EnemyState.DAMAGED);
        stateMachine.AddTransition<EnemyGrunt_Ranged, EnemyGrunt_Damaged>((int)EnemyState.DAMAGED);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Damaged>((int)EnemyState.DAMAGED);

        stateMachine.AddTransition<EnemyGrunt_Idle, EnemyGrunt_Death>((int)EnemyState.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Patrol, EnemyGrunt_Death>((int)EnemyState.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Chase, EnemyGrunt_Death>((int)EnemyState.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Melee, EnemyGrunt_Death>((int)EnemyState.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Ranged, EnemyGrunt_Death>((int)EnemyState.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Attack, EnemyGrunt_Death>((int)EnemyState.DEAD);
        stateMachine.AddTransition<EnemyGrunt_Damaged, EnemyGrunt_Death>((int)EnemyState.DEAD);

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
        if (PlayerKronoEnd.GetIsKronoEnd() && !isFrozen)
        {
            isFrozen = true;
            kronoEndData.wasStopped = navAgent.isStopped;
            navAgent.isStopped = true;
            return;
        }
        else if(!PlayerKronoEnd.GetIsKronoEnd() && isFrozen)
        {
            isFrozen = false;
            navAgent.isStopped = kronoEndData.wasStopped;
        }
        if(isFrozen)
        {
            return;
        }
        stateMachine.Update();
    }

    private void Initialize()
    {
        player = GameObject.FindWithTag("Player");
        navAgent = GetComponent<NavMeshAgent>();
        mesh = GetComponent<MeshRenderer>();
        originalColor = mesh.material.color;
        audioSource = GetComponent<AudioSource>();
        attackBox.SetActive(false);
        isInitialized = true;
        canPatrol = true;
        canAttack = true;
        canStrafe = true;
    }

    /// <summary>
    /// ステート変更とステート変数の更新
    /// </summary>
    /// <param name="state"></param>
    private void UpdateState(EnemyState state)
    {
        currentState = state;
        stateMachine.SendEvent((int)state);
    }

    /// <summary>
    /// 戦闘状態を更新（近距離遠距離状態にするか判断する）
    /// </summary>
    private void EvaluateCombatState()
    {
        int meleeCount = GetMeleeEnemies();
        float distanceToPlayer = Vector3.Distance(playerPos, transform.position);

        // すでに戦闘状態の場合
        if (playerDetected)
        {
            if (meleeCount == 0)
            {
                meleeState = true;

                if (distanceToPlayer < meleeCombatRange)
                {
                    UpdateState(EnemyState.MELEE);
                }
                else
                {
                    UpdateState(EnemyState.CHASE);
                }
                return;
            }

            if (meleeState)
            {
                if (distanceToPlayer < meleeCombatRange)
                {
                    UpdateState(EnemyState.MELEE);
                }
                else
                {
                    UpdateState(EnemyState.CHASE);
                }
            }
            else
            {
                if (distanceToPlayer < rangedCombatRange)
                {
                    UpdateState(EnemyState.MELEE);
                }
                else
                {
                    UpdateState(EnemyState.CHASE);
                }
            }
        }
        else
        {
            playerDetected = true;

            if (meleeCount == 0 || (distanceToPlayer < meleeCombatRange && meleeCount < 3))
            {
                meleeState = true;
                
                if (distanceToPlayer < meleeCombatRange)
                {
                    UpdateState(EnemyState.MELEE);
                }
                else
                {
                    UpdateState(EnemyState.CHASE);
                }
            }
            else
            {
                meleeState = false;

                if (distanceToPlayer < rangedCombatRange)
                {
                    UpdateState(EnemyState.RANGED);
                }
                else
                {
                    UpdateState(EnemyState.CHASE);
                }
            }
        }
    }

    /// <summary>
    /// プレイヤーが見えるか確認する
    /// </summary>
    private void CheckPlayerVisible()
    {
        Vector3 dirToPlayer = player.transform.position - transform.position;

        // 敵の視野内にいるかどうか
        if (!playerInSight)
        {
            // 頭を振り向く機能とかあれば頭を基準にしたい
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

            if (angleToPlayer > playerSearchAngle / 2f)
            {
                return;
            }
        }

        float playerCheckDistance = playerDetected ? combatDetectionDistance : playerSearchDistance;

        // 敵が直接見えるかどうか（壁などの障害がないか、遠すぎないか、視野内は考慮しない）
        bool playerVisible = !Physics.Linecast(transform.position, player.transform.position, 0) && dirToPlayer.magnitude < playerCheckDistance;

        // プレイヤーの最新のポジションを取得
        if (playerVisible)
        {
            playerPos = player.transform.position;
        }

        // 敵がプレイヤーをもう一度見つけた
        if (playerVisible && !playerInSight)
        {
            playerLoseDetectionCts?.Cancel();
            playerLoseDetectionCts?.Dispose();
            playerLoseDetectionCts = null;

            playerInSight = true;
            EvaluateCombatState();
        }
        // 敵がプレイヤーを見失った
        else if (!playerVisible && playerInSight)
        {
            playerLoseDetectionCts?.Cancel();
            playerLoseDetectionCts?.Dispose();

            playerInSight = false;
            playerLoseDetectionCts = new CancellationTokenSource();
            ResetPlayerDetection(playerLoseDetectionCts.Token).Forget();
            UpdateState(EnemyState.CHASE);
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
            UpdateState(EnemyState.DAMAGED);

            damageFlashCts?.Cancel();
            damageFlashCts?.Dispose();
            damageFlashCts = new CancellationTokenSource();
            DamageFlash(damageFlashCts.Token).Forget();

            Debug.Log($"Enemy [{gameObject.name}] took {damageValue} damage. HP {data.HP}/{data.maxHP}");
        }
        else
        {
            isDead = true;
            UpdateState(EnemyState.DEAD);
        }
        return isDead;
    }

    public override void Death()
    {
        DeathMotion().Forget();
        Debug.Log($"Enemy [{gameObject.name}] died");
    }

    public override EnemyType GetEnemyType()
    {
        return enemyType;
    }

    public override bool IsMeleeAttacking()
    {
        return isAttacking;
    }

    public override bool IsMeleeState()
    {
        return meleeState;
    }

    public override bool InCombat()
    {
        return playerDetected;
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

    /// <summary>
    /// 戦闘中の移動
    /// </summary>
    private void CombatMovement()
    {
        isStrafing = true;

        Vector3 strafeDir;
        Vector3 playerDir = playerPos - transform.position;
        float distance = playerDir.magnitude;
        float desiredDistance = (meleeState ? meleeCombatRange : rangedCombatRange) - combatRangeHalfLength;

        List<IEnemy> nearEnemies =  GetEnemiesInRange(transform.position, 3);

        if (nearEnemies.Count == 0)
        {
            strafeDir = Vector3.Cross(playerDir.normalized, Vector3.up) * (UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1);

            if (distance > desiredDistance + 0.5f)
            {
                strafeDir += playerDir.normalized;
            }
            else if (distance < desiredDistance - 0.5f)
            {
                strafeDir -= playerDir.normalized;
            }
        }
        else
        {
            Vector3 closestEnemy = new Vector3(0, -100000, 0);
            foreach (var enemy in nearEnemies)
            {
                if (Vector3.Distance(closestEnemy, transform.position) > Vector3.Distance(enemy.GetTransform().position, transform.position))
                {
                    closestEnemy = enemy.GetTransform().position;
                }
            }

            strafeDir = (transform.position - closestEnemy).normalized;
        }

        navAgent.destination = transform.position + strafeDir.normalized * 2f;
    }

    /// <summary>
    /// 近距離状態の敵の数を取得
    /// </summary>
    /// <returns></returns>
    private int GetMeleeEnemies()
    {
        int meleeCount = 0;
        List<IEnemy> enemyList = GetEnemiesInRange(playerPos, combatDetectionDistance);

        foreach (IEnemy enemy in enemyList)
        {
            if (enemy.InCombat() && enemy.IsMeleeState())
            {
                meleeCount++;
            }
        }

        return meleeCount;
    }

    /// <summary>
    /// 範囲内の敵を取得
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="checkRadius"></param>
    /// <returns></returns>
    private List<IEnemy> GetEnemiesInRange(Vector3 startPos, float checkRadius)
    {
        List<IEnemy> enemyList = new List<IEnemy>();
        Collider[] enemyColliders = Physics.OverlapSphere(startPos, checkRadius);

        foreach (Collider collider in enemyColliders)
        {
            IEnemy enemy = collider.gameObject.GetComponent<IEnemy>();
            if (enemy != null && collider.gameObject != gameObject)
            {
                enemyList.Add(enemy);
            }
        }

        return enemyList;
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
                await TimeControl.KronoYield(token);
            }

            isDamaged = false;

            if (playerDetected)
            {
                EvaluateCombatState();
            }
            else
            {
                UpdateState(EnemyState.IDLE);
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
            await TimeControl.KronoYield();
        }

        Destroy(gameObject);
    }

    private async UniTaskVoid MeleeAttackAnimation()
    {
        isAttacking = true;
        navAgent.SetDestination(playerPos);
        while (true)
        {
            if (Vector3.Distance(playerPos, transform.position) < meleeAttackRange)
            {
                break;
            }
            await TimeControl.KronoYield();
        }
        navAgent.isStopped = true;
        attackBox.SetActive(true);
        audioSource.PlayOneShot(swordClip);
        await TimeControl.KronoDelay(meleeAttackTime);
        attackBox.SetActive(false);
        isAttacking = false;
        ResetAttackCooldown(meleeAttackCooldown).Forget();
    }

    private async UniTaskVoid RangedAttackAnimation()
    {
        isAttacking = true;
        isRangedAttacking = true;
        navAgent.updateRotation = false;
        await TimeControl.KronoDelay(rangedAttackDelay);
        for (int i = 0; i < rangedAttackCount; i++)
        {
            await TimeControl.KronoDelay(rangedAttackInterval);
            audioSource.PlayOneShot(gunClip);
            ShootProjectile();
        }
        isRangedAttacking = false;
        navAgent.updateRotation = true;
        await TimeControl.KronoDelay(rangedAttackDelay);
        isAttacking = false;
        ResetAttackCooldown(rangedAttackCooldown).Forget();
    }

    private async UniTaskVoid ResetPatrol()
    {
        await TimeControl.KronoDelay(patrolWaitTime);
        canPatrol = true;
    }

    private async UniTaskVoid ResetAttackCooldown(float attackCooldown)
    {
        await TimeControl.KronoDelay(attackCooldown);
        canAttack = true;
    }

    private async UniTaskVoid ResetStrafeCooldown(float strafeCooldown)
    {
        await TimeControl.KronoDelay(strafeCooldown);
        canStrafe = true;
    }

    private async UniTaskVoid ResetPlayerDetection(CancellationToken token)
    {
        try
        {
            await TimeControl.KronoDelay(playerSearchTime, token);
            playerDetected = false;
            playerInSight = false;
            UpdateState(EnemyState.IDLE);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("正常に中断");
        }
    }
}
