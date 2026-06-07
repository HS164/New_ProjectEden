using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public partial class PlayerController : MonoBehaviour, IDamageable, IPlayer
{
    [SerializeField] private Rigidbody rbody;

    // 移動速度に関するパラメータ
    [SerializeField] private MoveSensitivity moveSensitivity;
    // ジャンプに関するパラメータ
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private Vector3 autoJumpDetectRange;
    [SerializeField] private float ascendingGravityScale;
    [SerializeField] private float descendingGravityScale;
    private float gravityScale = 1;
    private float initialGravityScale = 1;
    // エアアクセルに関するパラメータ
    [SerializeField] private PlayerAirAccele playerAirAccele;

    [SerializeField] private Transform cameraObj;
    [SerializeField] private float searchRadius = 10f;
    [SerializeField, ReadOnly] private float attackRange = 1f;

    // 武器を拾うシステム
    [SerializeField] private WeaponManager weaponManager;
    private SelectableWeaponBase currentWeapon = null;

    // ワープターゲットを可視化する線
    [SerializeField] private LineRenderer lineRenderer;

    // 幻影残身 の値
    [SerializeField] private GameObject playerShadowPrefab;
    [SerializeField] private float shadowSpawnInterval = 1;

    // タイムシフトステップ　の値
    [SerializeField] private GameObject timeShiftEffect;
    [SerializeField] private float timeShiftDuration = 5f;
    [SerializeField] private float playerTimeScale = 0.85f;
    [SerializeField] private float shiftTimeScale = 0.5f;
    private bool isTimeShifting = false;

    // 攻撃後隙の時間
    [SerializeField] private float attackCoolTime;
    [SerializeField] private GameObject attackBox;
    private PlayerAttackCoolTimer attackableTimer;

    [SerializeField] private float overDriveTime = 10f;
    [SerializeField] private float kronoEndTime = 7f;
    [SerializeField] private float warpFloatTime = 1f;
    private PlayerKronoEnd kronoEnd;
    private bool isKronoEnd = false;

    // パラメータ
    [SerializeField] private float maxHP;
    [SerializeField, ReadOnly] private float currentHP;
    [SerializeField, ReadOnly] private float atk = 5;

    private Vector3 horizontalVelocity = Vector3.zero;
    // 接触しているオブジェクトの法線ベクトルをMap形式で管理、コライダーのIDをkeyとして扱う
    // 変数の再代入ができないようにreadonlyで宣言(Javaのfinal)
    private readonly Dictionary<int, Vector3> wallContactNormals = new Dictionary<int, Vector3>();

    private bool isDead = false;
    private bool isStunned = false;
    private bool isTargetingEnemy = false;
    private bool isTargetingWeapon = false;
    private bool autoJumpFlag = false;

    private PlayerInput_Controller.PlayerInputActions input;

    // ターゲットとなるオブジェクトを取得
    SelectableObjectManager selectableManager;

    CancellationTokenSource jumpCTS;

    public float CurrentHp
    {
        get => currentHP;
        set => currentHP = value;
    }

    public float MaxHp
    {
        get => maxHP;
        set => maxHP = value;
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
        currentHP = maxHP;
        moveSensitivity.Init();
        playerJump.Reset();
        playerAirAccele.Reset();
        isTargetingWeapon = true;

        rbody.useGravity = false;

        attackableTimer = new PlayerAttackCoolTimer(attackCoolTime, overDriveTime);
        attackableTimer.SetPlayer(this.gameObject.transform);
        attackBox.SetActive(false);
        kronoEnd = new PlayerKronoEnd(kronoEndTime);

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.yellow;
        lineRenderer.enabled = false;

        selectableManager = SelectableObjectManager.instance;
        selectableManager.SetTargetType(SelectableType.GIMMICK);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        input = new PlayerInput_Controller().PlayerInput;
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Update()
    {
        if (isDead) 
        {
            return;
        }

        if (input.Jump.WasPressedThisFrame() && playerJump.IsJump() && !playerJump.CanJump() && IsNearLand())
        {
            autoJumpFlag = true;
            Debug.Log("prep jump3");
        }

        if (IsLand())
        {
            playerJump.Reset();
            playerAirAccele.Reset();
            gravityScale = initialGravityScale;

            if (autoJumpFlag)
            {
                rbody.linearVelocity = new Vector3(rbody.linearVelocity.x, 0, rbody.linearVelocity.z);
                playerJump.CountUpJump();
                Jump();
                autoJumpFlag = false;
            }
        }

        if (isStunned)
        {
            return;
        }

        // new inputs

        if (currentWeapon != null)
        {
            if (input.WeaponWarp.IsPressed() && !isTargetingEnemy && !isTargetingWeapon)
            {
                isTargetingWeapon = true;
                selectableManager.SetTargetType(SelectableType.GIMMICK);
            }
            else if (input.EnemyWarp.IsPressed() && !isTargetingEnemy && !isTargetingWeapon && currentWeapon is not GunWeapon)
            {
                isTargetingEnemy = true;
                selectableManager.SetTargetType(SelectableType.ENEMY);
            }

            if (input.WeaponWarp.WasReleasedThisFrame() && isTargetingWeapon)
            {
                isTargetingWeapon = false;
                selectableManager.SetTargetType(SelectableType.NONE);
            }
            else if(input.EnemyWarp.WasReleasedThisFrame() && isTargetingEnemy)
            {
                isTargetingEnemy = false;
                selectableManager.SetTargetType(SelectableType.NONE);
            }
        }

        if (input.Attack.WasPressedThisFrame())
        {
            if (currentWeapon == null)
            {
                // weapon warp check
                TelepotationTarget();
                return;
            }

            if (isTargetingWeapon)
            {
                // warp check
                TelepotationTarget();
                return;
            }
            else if (isTargetingEnemy)
            {
                // enemy warp check
                TelepotationTarget();
                return;
            }

            Attack();
        }


        if (playerJump.IsJump())
        {
            if (playerAirAccele.CanAction() && input.AirAccele.WasPressedThisFrame())
            {
                playerAirAccele.CountUpAction();
                AirAccele();
            }
        }
        if (input.Move.ReadValue<Vector2>().magnitude > 0.1f)
        {
            Move();
        }
        if (input.Jump.WasPressedThisFrame())
        {
            if (playerJump.CanJump())
            {
                playerJump.CountUpJump();
                Jump();
            }
        }

        if (input.TimeShift.WasPressedThisFrame() && !isTimeShifting)
        {
            TimeShift().Forget();
        }

        // 移動スティックを話しているときの処理
        if (input.Move.ReadValue<Vector2>().magnitude < 0.1f)
        {
            moveSensitivity.Reset();
            // ここでVector3.zeroに向かって少しずつ減速
            // y軸の移動のみ、linearVelocityを使用して落下などの挙動が損なわれないようにしている
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, moveSensitivity.DecelerationRate * Time.deltaTime);
            rbody.linearVelocity = new Vector3(horizontalVelocity.x, rbody.linearVelocity.y, horizontalVelocity.z);
        }

        SearchTarget();

        // テストコード
        // プロトタイプでスキルの起動を行うためのコード
        // ---------------------------ここから---------------------------
        // オーバードライブ
        if (input.OverDrive.WasPressedThisFrame())
        {
            ActiveOverDrive();
        }

        // クロノ・エンド
        if (input.ChronoEnd.WasPressedThisFrame())
        {
            ActiveKronoEnd();
        }
        // ---------------------------ここまで---------------------------
    }

    private void FixedUpdate()
    {
        rbody.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }

    async UniTask WarpFloat()
    {
        float endTime = Time.time + warpFloatTime;

        // Use while to wait until time passes
        while (Time.time < endTime)
        {
            if (input.Move.IsPressed())
            {
                break;
            }
            await UniTask.Yield();
        }
        rbody.isKinematic = false;
    }

    /// <summary>
    /// プレイヤーの水平移動を処理する。加速・減速・反転摩擦・壁スライド・スムーズ回転を含む
    /// </summary>
    private void Move()
    {
        // 入力方向をカメラ基準のワールド座標へ変換
        Vector2 moveAmt = input.Move.ReadValue<Vector2>();
        Vector3 inputDir = new Vector3(moveAmt.x, 0, moveAmt.y);
        Quaternion camRot = Quaternion.Euler(0, cameraObj.transform.eulerAngles.y, 0);
        // 入力情報とカメラの向きから実際に進みたい方向を決定
        Vector3 desiredDirection = camRot * inputDir;
        float accelRate = moveSensitivity.AccelerationRate;

        // タイムスケール補正：スロー状態でもプレイヤー本人は通常速度を維持する
        float speedMultiplier = 1f;
        if (isTimeShifting)
        {
            speedMultiplier = playerTimeScale / Time.timeScale;
        }
        if (isKronoEnd)
        {
            speedMultiplier = 1.0f / Time.timeScale;
        }

        Vector3 targetVelocity = desiredDirection * moveSensitivity.MaxSpeed * speedMultiplier;

        // 反転入力時は摩擦倍率を加えて素早く切り返す
        // Vector3.Dotで二つのベクトルの比較、値が -1 に近いほど切り返しが行われたと判定できる
        if (horizontalVelocity.sqrMagnitude > 0.01f &&
            Vector3.Dot(horizontalVelocity.normalized, desiredDirection.normalized) < -0.1f)
        {
            accelRate *= moveSensitivity.ReversalFriction;
        }

        // 第1引数から第2引数に向かって、第3引数の距離だけ近づける関数
        // 毎フレーム少しずつ targetVelocity に近づくことで、滑らかな加速が生まれる。
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, accelRate * Time.deltaTime);

        // 壁スライド：壁への押し込み成分のみ除去して壁面に沿って滑る
        // Vector3.ProjectOnPlane(velocity, normal) とは： 速度ベクトルから「壁の法線方向の成分だけを取り除く」関数
        // 壁に向かって走っても「壁に突っ込む分の速度」が消えて「壁に平行な分だけ」が残るため、これによって壁に沿って滑るように動ける。
        Vector3 finalVelocity = horizontalVelocity;
        foreach (KeyValuePair<int, Vector3> kv in wallContactNormals)
        {
            if (Vector3.Dot(finalVelocity, kv.Value) < 0)
            {
                finalVelocity = Vector3.ProjectOnPlane(finalVelocity, kv.Value);
            }
        }

        rbody.linearVelocity = new Vector3(finalVelocity.x, rbody.linearVelocity.y, finalVelocity.z);

        // 移動入力がある間はスムーズに向きを変える
        if (desiredDirection.sqrMagnitude > 0.01f)
        {
            // 瞬時に向きを変えるのではなく、RotateTowardsを使用して、滑らかに向きが変わるように修正
            Quaternion targetRot = Quaternion.LookRotation(desiredDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, moveSensitivity.TurnSpeed * Time.deltaTime);
        }
    }

    private void Attack()
    {
        if (attackableTimer.nonAttackable)
        {
            return;
        }

        Debug.Log("Attack");
        var hits = Physics.BoxCastAll(
            transform.position + transform.forward,
            Vector3.one * (attackRange / 2),
            transform.forward,
            Quaternion.identity,
            attackRange)
            .Select(hit => hit.transform)
            .Where(hit => hit.GetComponent<IDamageable>() != null)
            .Where(hit => hit != transform)
            .ToList();
        foreach(var hit in hits)
        {
            Debug.Log(hit.gameObject.name);
            var damageObj = hit.GetComponent<IDamageable>();
            var death = damageObj.Damage(atk, transform.position);
            // 神速パワーアップ
            PlayerPowerManager.Instance.ChargeGauge();
            // スピードリンク発動
            moveSensitivity.SpeedUp();
            if(death)
            {
                var selectableObj = hit.GetComponent<IPlayerSelectable>();
                if (selectableObj != null)
                {
                    selectableObj.OnRelease();
                }
                damageObj.Death();
            }
        }

        // 攻撃中に、攻撃処理が行われないようにするための待機処理
        attackableTimer.StartAttackCoolDown();

        // 攻撃アニメーションの終了を待機する
        ViewAttackAnimation().Forget();
    }

    private void Jump()
    {
        Debug.Log("Jump");
        rbody.AddForce(new Vector3(0, playerJump.Power, 0), ForceMode.Impulse);

        jumpCTS?.Cancel();
        jumpCTS?.Dispose();
        jumpCTS = new CancellationTokenSource();
        gravityScale = ascendingGravityScale;
        JumpGravity(jumpCTS.Token).Forget();

        playerJump.DelayGroundJudgement().Forget();
    }

    // ジャンプの上昇中
    private async UniTaskVoid JumpGravity(CancellationToken token)
    {
        await UniTask.WaitForSeconds(0.5f, cancellationToken: token);
        while (rbody.linearVelocity.y > 0)
        {
            await UniTask.Yield();
        }
        gravityScale = descendingGravityScale;
    }

    private bool IsLand()
    {
        if(!playerJump.CanGroundJudgement())
        {
            return false;
        }

        var hits = Physics.BoxCastAll(
            transform.position - transform.up,
            Vector3.one,
            -transform.up,
            Quaternion.identity,
            0.01f)
            .Select(hit => hit.transform)
            .Where(hit => hit.GetComponent<IPlatformer>() != null)
            .ToList();
        return hits.Count > 0;
    }

    // 先行入力判定の簡易版
    private bool IsNearLand()
    {
        if (!playerJump.CanGroundJudgement())
        {
            return false;
        }

        var hits = Physics.BoxCastAll(
            transform.position - transform.up - autoJumpDetectRange,
            Vector3.one * 2,
            -transform.up,
            Quaternion.identity,
            1f)
            .Select(hit => hit.transform)
            .Where(hit => hit.GetComponent<IPlatformer>() != null)
            .ToList();
        return hits.Count > 0;
    }

    public bool Damage(float damage, Vector3 hitPosition)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            return true;
        }

        isStunned = true;

        rbody.AddForce((transform.position + Vector3.up - hitPosition).normalized * 5f, ForceMode.Impulse);

        ResetHitStun(0.2f).Forget();

        return false;
    }

    public void Death()
    {
        isDead = true;
        attackableTimer?.OnDestroy();
        kronoEnd?.OnDestroy();
    }

    private void Teleportation(Transform targetObj)
    {
        Vector3 currentPos = transform.position;
        Vector3 warpPos;
        rbody.linearVelocity = Vector3.zero;
        horizontalVelocity = Vector3.zero;
        moveSensitivity.Reset();
        var dir = new Vector3(targetObj.position.x, 0, targetObj.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        transform.rotation = Quaternion.LookRotation(dir);
        warpPos = targetObj.position - dir.normalized;
        transform.position = warpPos;
        WarpShadow(currentPos, warpPos);
    }

    /// <summary>
    /// 空中での追加加速（エアアクセル）処理。カメラから見てプレイヤーの前方にブーストをかける
    /// </summary>
    private void AirAccele()
    {
        // カメラから見たプレイヤーの方向を取得
        var dir = new Vector3(transform.position.x - cameraObj.transform.position.x, 0, transform.position.z - cameraObj.transform.position.z);
        // ブースト値を計算（y軸は落下などの重力処理にかかわるため計算しない）
        var boost = new Vector3(dir.x, 0, dir.z) * playerAirAccele.Power;

        // ブーストを水平速度に加算して即時反映
        // AddForceだとlinearVelocityを書き換えている影響で反映されないため直接計算
        horizontalVelocity += boost;

        // ブースト値を加算した平行ベクトル、y軸は重力計算に任せる
        rbody.linearVelocity = new Vector3(horizontalVelocity.x, rbody.linearVelocity.y, horizontalVelocity.z);
        transform.rotation = Quaternion.LookRotation(dir);

        // 上昇重力スケールに切り替えて頂点まで滑らかに上がる
        jumpCTS?.Cancel();
        jumpCTS?.Dispose();
        jumpCTS = new CancellationTokenSource();
        gravityScale = ascendingGravityScale;
        JumpGravity(jumpCTS.Token).Forget();
    }

    // ターゲットを取得
    private void SearchTarget()
    {
        var target = selectableManager.GetTargetObject();
        if (target != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.transform.position);
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    private void TelepotationTarget()
    {
        if (attackableTimer.nonAttackable)
        {
            return;
        }

        var target = selectableManager.GetTargetObject();

        if (target != null)
        {
            selectableManager.SelectTarget(target);
            Teleportation(target);
            var weapon = target.GetComponent<IWeaponAccessor>();
            if (weapon != null)
            {
                weaponManager.ChangeWeapon(weapon);
                currentWeapon = target.GetComponent<SelectableWeaponBase>();
                var weaponData = currentWeapon.GetWeaponData();
                if (weaponData != null)
                {
                    atk = weaponData.damage;
                    attackRange = weaponData.attackRange;
                }
                isTargetingWeapon = false;
                selectableManager.SetTargetType(SelectableType.NONE);
            }
            else
            {
                Attack();
                rbody.isKinematic = true;
                WarpFloat().Forget();
                isTargetingEnemy = false;
                selectableManager.SetTargetType(SelectableType.NONE);
            }
        }
    }

    /// <summary>
    /// 壁に接触している間、法線を収集して壁スライドに使用する
    /// OnCllisionStayは接触しているコライダーごとに呼び出しが行われる
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        Vector3 wallNormal = Vector3.zero;
        int count = 0;
        
        // 接触点から法線を取得、壁スライドできるようにMapに追加
        foreach (ContactPoint contact in collision.contacts)
        {
            // y成分が小さい面のみ壁として扱う（床・天井を除外するための閾値）
            // ※床・天井の法線は(0, 1, 0)だったり、(0, -1, 0)等になるため絶対値0.7以上を除外
            if (Mathf.Abs(contact.normal.y) < 0.7f)
            {
                wallNormal += contact.normal;
                count++;
            }
        }

        // コライダーごとにOnCollistionStayが呼び出されるため、壁の判定として必要な法線情報のみ追加する
        if (count > 0)
        {
            // Dictionaryへの追加（Mapでいうput(key, value)と同じ）
            // ベクトルの長さを1にするためにwallNormal / countを行う
            wallContactNormals[collision.collider.GetInstanceID()] = (wallNormal / count).normalized;
        }
        else
        {
            // 床・天井のみの接触になったときにMapから除外
            wallContactNormals.Remove(collision.collider.GetInstanceID());
        }
    }

    /// <summary>
    /// 壁との接触が終了したら法線情報を削除する
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        wallContactNormals.Remove(collision.collider.GetInstanceID());
    }

    // このメソッドは、スクリプトが付いたオブジェクトがSceneビューで選択されているときに呼び出されます
    void OnDrawGizmosSelected()
    {
        if (cameraObj == null)
        {
            return;
        }

        // Gizmosの色を黄色に設定
        Gizmos.color = Color.yellow;
        
        // SphereCastの開始位置とレイの方向を可視化
        Gizmos.DrawRay(transform.position, cameraObj.transform.forward * 0.01f);
        
        // SphereCastの球体を可視化
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        // SphereCastの立方体を可視化
        Gizmos.DrawWireCube(transform.position + transform.forward, Vector3.one * attackRange);

        // SphereCastの立方体(着地判定)を可視化
        Gizmos.DrawWireCube(transform.position - transform.up, Vector3.one);
    }

    /// <summary>
    /// ワープした際に初期地点とワープ先の間に残像を配置
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    void WarpShadow(Vector3 startPos, Vector3 endPos) 
    {
        if (!PlayerPowerManager.Instance.HasPowerLevel(PlayerPowerEnum.LIGHTNING))
        {
            return;
        }
        Vector3 warpDir = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);
        float spawnDistance = 0;
        Vector3 spawnPos = Vector3.zero;

        while (spawnDistance < distance)
        {
            spawnPos = startPos + warpDir * spawnDistance;
            Instantiate(playerShadowPrefab, spawnPos, transform.rotation);
            spawnDistance += shadowSpawnInterval;
        }
    }

    /// <summary>
    /// 走り状態の時にプレイヤーの残像を作る
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid TimeShift()
    {
        if (!PlayerPowerManager.Instance.HasPowerLevel(PlayerPowerEnum.LIGHTNING))
        {
            return;
        }
        isTimeShifting = true;
        Time.timeScale = shiftTimeScale;
        GameObject prefab = Instantiate(timeShiftEffect, transform.position, Quaternion.identity);
        await UniTask.Delay(TimeSpan.FromSeconds(timeShiftDuration), true);
        isTimeShifting = false;
        Time.timeScale = 1.0f;
        Destroy(prefab);
    }

    /// <summary>
    /// 攻撃状態の可視化の制御
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid ViewAttackAnimation()
    {
        // 攻撃判定の可視化（攻撃アニメーションがある場合、それを再生）
        attackBox.SetActive(true);
        float interval = attackCoolTime / 5;
        float timeoutTimer = 0;

        while (attackableTimer.nonAttackable)
        {
            // 攻撃不可能な時間は待機し続ける
            await UniTask.Delay(TimeSpan.FromSeconds(interval), true);
            timeoutTimer += interval;
            // タイムアウトしたらループを抜ける
            if (timeoutTimer > attackCoolTime + 1f) break;
        }

        attackBox.SetActive(false);
    }

    // オーバードライブ起動
    private void ActiveOverDrive()
    {
        if (!PlayerPowerManager.Instance.HasPowerLevel(PlayerPowerEnum.GOD))
        {
            return;
        }
        attackableTimer.ChangeOverDrive();
    }

    /// <summary>
    /// スタンのクールタイム
    /// </summary>
    /// <param name="strafeCooldown"></param>
    /// <returns></returns>
    private async UniTaskVoid ResetHitStun(float hitStunTime)
    {
        await UniTask.WaitForSeconds(hitStunTime);
        horizontalVelocity = new Vector3(rbody.linearVelocity.x, 0, rbody.linearVelocity.z);
        isStunned = false;
    }

    // クロノ・エンド起動
    private async void ActiveKronoEnd()
    {
        Debug.Log("isKronoEnd : " + isKronoEnd);
        if (!PlayerPowerManager.Instance.HasPowerLevel(PlayerPowerEnum.GOD) || PlayerKronoEnd.GetIsKronoEnd())
        {
            // まだ神速の段階ではない、
            // もしくはスキル使用中であれば即終了
            return;
        }
        isKronoEnd = true;

        // 結果が返ってくるまで待機
        bool finishKronoEnd = await kronoEnd.StartKronoEnd();

        if (finishKronoEnd)
        {
            Debug.Log("finish Krono End");
            // 終了時、HPの半分のダメージをくらう
            float damage = currentHP / 2;
            Damage(damage, transform.position);
        }

        isKronoEnd = false;
    }
}
