using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine;

public partial class PlayerController : MonoBehaviour, IDamageable, IPlayer
{
    [SerializeField] private Rigidbody rbody;

    // 移動速度に関するパラメータ
    [SerializeField] private MoveSensitivity moveSensitivity;
    // ジャンプに関するパラメータ
    [SerializeField] private PlayerJump playerJump;
    // エアアクセルに関するパラメータ
    [SerializeField] private PlayerAirAccele playerAirAccele;

    [SerializeField] private Transform cameraObj;
    [SerializeField] private float searchRadius = 10f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float dashSpeed = 1.5f;

    // 幻影残身 の値
    [SerializeField] private GameObject playerShadowPrefab;
    [SerializeField] private float shadowSpawnInterval = 1;
    private bool isDashing = false;

    // タイムシフトステップ　の値
    [SerializeField] private GameObject timeShiftEffect;
    [SerializeField] private float timeShiftDuration = 5f;
    [SerializeField] private float playerTimeScale = 0.85f;
    [SerializeField] private float shiftTimeScale = 0.5f;
    private bool isTimeShifting = false;

    // 攻撃後隙の時間
    [SerializeField] private float attackCoolTime;
    private PlayerAttackCoolTimer attackableTimer;

    // パラメータ
    private float maxHP = 10f;
    private float currentHP;
    private float atk = 5;

    private bool isFixed = false;
    private bool isDead = false;

    private PlayerInput_Controller.PlayerInputActions input;

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

        attackableTimer = new PlayerAttackCoolTimer(attackCoolTime);
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
        if(isDead) 
        {
            return;
        }

        if (IsLand())
        {
            playerJump.Reset();
            playerAirAccele.Reset();
        }

        // プレイヤーが固定されているとき(密着時)
        if (!isFixed)
        {
            if (playerJump.IsJump())
            {
                if (playerAirAccele.CanAction() && input.AirAccele.WasPressedThisFrame())
                {
                    playerAirAccele.CountUpAction();
                    AirAccele();
                }
            }
            else
            {
                if (input.Move.ReadValue<Vector2>().magnitude > 0.1f)
                {
                    Move();
                }
            }

            if (input.Jump.WasPressedThisFrame())
            {
                if (playerJump.CanJump())
                {
                    playerJump.CountUpJump();
                    Jump();
                }
            }
            if (input.Sprint.WasPressedThisFrame())
            {
                Debug.Log("ダッシュ開始");

                isDashing = true;
            }
            if(input.Sprint.WasReleasedThisFrame())
            {
                Debug.Log("ダッシュ中断");

                isDashing = false;
            }
            if(Input.GetKeyDown(KeyCode.M) && !isTimeShifting)
            {
                TimeShift().Forget();
            }
        }
        else
        {
            if(input.Attack.WasPressedThisFrame())
            {
                Attack();
            }
            if (input.ReleaseTarget.WasPressedThisFrame())
            {
                ReleaseTarget();
            }
            if (input.Sprint.WasReleasedThisFrame())
            {
                Debug.Log("ダッシュ中断");

                isDashing = false;
            }
        }

        SearchTarget();

        // テストコード
        // 現実装では段階を上げる処理が入っていないため、
        // プロトタイプでスキルの起動を行うためのコード
        // ---------------------------ここから---------------------------
        if (Input.GetKeyDown(KeyCode.F))
        {
            attackableTimer.ChangeOverDrive(true);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            attackableTimer.ChangeOverDrive(false);
        }
        // ---------------------------ここまで---------------------------
    }

    // プレイヤー移動
    private void Move()
    {
        var moveAmt = input.Move.ReadValue<Vector2>();
        var inputDir = new Vector3(moveAmt.x, 0, moveAmt.y);
        // カメラと同期させてはいけない　させると移動中にカメラを動かすとカクカクになる
        var dir = new Vector3(transform.position.x - cameraObj.transform.position.x, 0, transform.position.z - cameraObj.transform.position.z);
        var rot = Quaternion.LookRotation(dir);
        /****              ここまで                ****/
        var moveDirection = rot * inputDir;
        if(isDashing)
        {
            moveDirection *= dashSpeed;
        }
        if(isTimeShifting)
        {
            moveDirection *= playerTimeScale / Time.timeScale;
        }
        var moveVelocity = moveDirection * moveSensitivity.Sensitivity;
        rbody.linearVelocity = new Vector3(moveVelocity.x, rbody.linearVelocity.y, moveVelocity.z);
        transform.rotation = Quaternion.LookRotation(moveDirection);
        //Debug.Log(rbody.linearVelocity);
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
            var death = damageObj.Damage(atk);
            // スピードリンク発動
            moveSensitivity.SpeedUp();
            if(death)
            {
                var selectableObj = hit.GetComponent<IPlayerSelectable>();
                if (selectableObj != null)
                {
                    selectableObj.OnRelease();
                    isFixed = false;
                    Debug.Log("fixed");
                }
                damageObj.Death();
            }
        }

        // 攻撃の後隙を開始する
        attackableTimer.StartAttackCoolDown();
    }

    private void Jump()
    {
        Debug.Log("Jump");
        rbody.AddForce(new Vector3(0, playerJump.Power, 0), ForceMode.Impulse);
    }

    private bool IsLand()
    {
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

    private void ReleaseTarget()
    {
        SelectableObjectManager.instance.ReleaseTarget();
        isFixed = false;
    }

    public bool Damage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            return true;
        }
        return false;
    }

    public void Death()
    {
        isDead = true;
    }

    private void Teleportation(Transform targetObj)
    {
        Vector3 currentPos = transform.position;
        Vector3 warpPos;
        rbody.linearVelocity = Vector3.zero;
        var dir = new Vector3(targetObj.position.x, 0, targetObj.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        transform.rotation = Quaternion.LookRotation(dir);
        warpPos = targetObj.position - dir.normalized;
        transform.position = warpPos;
        WarpShadow(currentPos, warpPos);
        isFixed = true;
    }

    private void AirAccele()
    {
        var dir = new Vector3(transform.position.x - cameraObj.transform.position.x, 0, transform.position.z - cameraObj.transform.position.z);
        rbody.AddForce(dir * playerAirAccele.Power, ForceMode.Impulse);
        transform.rotation = Quaternion.LookRotation(dir);
    }

    // ターゲットを取得
    private void SearchTarget()
    {
        // ターゲットとなるオブジェクトを取得
        var selectableManager = SelectableObjectManager.instance;
        var target = selectableManager.GetTargetObject();
        bool isInteract = false;
        if (isFixed)
        {
            isInteract = input.Interact.WasPressedThisFrame();
        }
        else
        {
            isInteract = input.Attack.WasPressedThisFrame();
        }
        if (isInteract)
        {
            if (target != null)
            {
                var currentSelect = selectableManager.SelectTarget(target);
                Teleportation(target);
                Attack();
            }
        }
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
        isTimeShifting = true;
        Time.timeScale = shiftTimeScale;
        GameObject prefab = Instantiate(timeShiftEffect, transform.position, Quaternion.identity);
        await UniTask.Delay(TimeSpan.FromSeconds(timeShiftDuration), true);
        isTimeShifting = false;
        Time.timeScale = 1.0f;
        Destroy(prefab);
    }
}
