using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngineInternal;

public partial class PlayerController : MonoBehaviour, IDamageable, IPlayer
{
    [SerializeField] private Rigidbody rbody;
    [SerializeField] private float moveSensitivity = 10f;
    [SerializeField] private Transform cameraObj;
    [SerializeField] private float searchRadius = 10f;
    [SerializeField] private float attackRange = 1f;

    // 幻影残身 の値
    [SerializeField] private GameObject playerShadowPrefab;
    [SerializeField] private float shadowSpawnInterval = 0.15f;
    [SerializeField] private float dashSpeed = 1.5f;
    private bool isDashing = false;
    private CancellationTokenSource shadowDashCts;

    // タイムシフトステップ　の値
    [SerializeField] private GameObject timeShiftEffect;
    [SerializeField] private float timeShiftDuration = 5f;
    [SerializeField] private float playerTimeScale = 0.85f;
    [SerializeField] private float shiftTimeScale = 0.5f;
    private bool isTimeShifting = false;

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
        if (!isFixed)
        {
            if (input.Move.ReadValue<Vector2>().magnitude > 0.1f)
            {
                Move();
            }
            if (input.Jump.WasPressedThisFrame())
            {
                Jump();
            }
            if (input.Sprint.WasPressedThisFrame())
            {
                Debug.Log("ダッシュ開始");

                isDashing = true;
                shadowDashCts = new CancellationTokenSource();
                ShadowDash(shadowDashCts.Token).Forget();
            }
            if(input.Sprint.WasReleasedThisFrame())
            {
                Debug.Log("ダッシュ中断");

                isDashing = false;
                shadowDashCts?.Cancel();
                shadowDashCts?.Dispose();
            }
            if(Input.GetKeyDown(KeyCode.M) && !isTimeShifting)
            {
                ShadowDash().Forget();
            }
        }
        else
        {
            if(input.Attack.WasPressedThisFrame())
            {
                Attack();
            }
            if (input.Jump.WasPressedThisFrame())
            {
                ReleaseTarget();
            }
            if (input.Sprint.WasReleasedThisFrame())
            {
                Debug.Log("ダッシュ中断");

                isDashing = false;
                shadowDashCts?.Cancel();
                shadowDashCts?.Dispose();
            }
        }

        SearchTarget();
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
        rbody.linearVelocity = moveDirection * moveSensitivity;
        transform.rotation = Quaternion.LookRotation(moveDirection);
        //Debug.Log(rbody.linearVelocity);
    }

    private void Attack()
    {
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
    }

    private void Jump()
    {
        Debug.Log("Jump");
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
        rbody.linearVelocity = Vector3.zero;
        var dir = new Vector3(targetObj.position.x, 0, targetObj.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        transform.rotation = Quaternion.LookRotation(dir);
        transform.position = targetObj.position - dir.normalized;
        isFixed = true;
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
    }

    /// <summary>
    /// 走り状態の時にプレイヤーの残像を作る
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async UniTaskVoid ShadowDash(CancellationToken token)
    {
        try
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(shadowSpawnInterval), true, cancellationToken: token);
                Instantiate(playerShadowPrefab, transform.position, transform.rotation);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("ダッシュ中断");
        }
    }

    /// <summary>
    /// 走り状態の時にプレイヤーの残像を作る
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async UniTaskVoid ShadowDash()
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
