using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectableObjectManager : MonoBehaviour
{
    [SerializeField] private Transform centerObject;
    [SerializeField] private Transform cameraObject;

    // シングルトンインスタンス
    public static SelectableObjectManager instance { get; private set; }

    private Transform targetObject = null;
    // 前回の選択可能オブジェクトを保持
    private IPlayerSelectable beforeWatched = null;
    // 前回選択したオブジェクトを保持
    private IPlayerSelectable beforeSelected = null;

    private void Awake()
    {
        // インスタンスがまだ存在しない場合、このインスタンスをシングルトンにする
        if (instance == null)
        {
            instance = this;
            // シーンを切り替えてもこのオブジェクトを破棄しないようにする
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 既にインスタンスが存在する場合、このオブジェクトを破棄する
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (centerObject == null)
        {
            Debug.LogError("centerObject is null");
        }
        if (cameraObject == null)
        {
            Debug.LogError("cameraObject is null");
        }
        targetObject = SearchCenterTarget();
    }

    public Transform GetTargetObject()
    {
        return targetObject;
    }

    public IPlayerSelectable GetBeforeWatched()
    {
        return beforeWatched;
    }

    public IPlayerSelectable GetBeforeSelected()
    {
        return beforeSelected;
    }

    // カメラ上のオブジェクトを選択するメソッド
    private Transform SearchCenterTarget()
    {
        float search_radius = 10f;
        // center指定した大正からSphereCastでhitしたものを取得
        var hits = Physics.SphereCastAll(
            centerObject.transform.position,
            search_radius,
            cameraObject.transform.forward,
            0.01f
        ).Select(hit => hit.transform).ToList();

        // そのなかの対象のものだけを再取得
        hits = FilterTargetObject(hits);

        // 対象オブジェクトがなければ、元に戻して終了
        if (hits.Count <= 0)
        {
            if (beforeWatched != null)
            {
                if (beforeWatched is UnityEngine.Object obj && obj != null)
                {
                    // 非同期処理でオブジェクトが削除された後に参照しようとしてエラーが発生する
                    // そのため、オブジェクトが削除済みかどうか判定する処理を行う
                    beforeWatched.AllowSelection(false);
                    beforeWatched = null;
                }
            }
            return null;
        }

        float minTargetDistance = float.MaxValue;
        Transform target = null;
        // 複数hitしたオブジェクトのうち繰り返し処理で近いものを探す
        foreach (var hit in hits)
        {
            Vector3 targetScreenPos = Camera.main.WorldToViewportPoint(hit.position);
            float targetDistance = Vector2.Distance(new Vector2(0.5f, 0.5f),
                new Vector2(targetScreenPos.x, targetScreenPos.y));
            if (targetDistance < minTargetDistance)
            {
                minTargetDistance = targetDistance;
                target = hit.transform;
            }
        }
        //Debug.Log(target.name);

        // 現在見ているものを設定する
        var currentWatch = target.GetComponent<IPlayerSelectable>();
        // 現在見ているオブジェクトに対して処理を行う
        SelectionPrediction(currentWatch);

        return target;
    }

    private List<Transform> FilterTargetObject(List<Transform> hits)
    {
        return hits
        .Where(hit =>
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(hit.position);
            return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        })
        .Where(hit => hit.GetComponent<IPlayerSelectable>() != null)
        .Where(hit => !hit.GetComponent<IPlayerSelectable>().IsSelect)
        .ToList();
    }

    // 選択可能なオブジェクトに対して行う処理
    private void SelectionPrediction(IPlayerSelectable nowWatch)
    {
        // 前回見ていたオブジェクトが存在しているかの判定
        bool existBeforeWatched = beforeWatched != null && (beforeWatched is UnityEngine.Object obj && obj != null);

        if (existBeforeWatched && beforeWatched != nowWatch)
        {
            // 前回のオブジェクトと今回のオブジェクトが違う場合
            // 選択候補になっているオブジェクトを入れ替え
            beforeWatched.AllowSelection(false);
            nowWatch.AllowSelection(true);
            beforeWatched = nowWatch;
        }
        else if (!existBeforeWatched)
        {
            // 新しくオブジェクトをセット
            nowWatch.AllowSelection(true);
            beforeWatched = nowWatch;
        }
    }

    // 渡されたターゲットの選択時処理を行う
    public IPlayerSelectable SelectTarget(Transform target)
    {
        if (target == null)
        {
            Debug.Log("target is null");
            return null;
        }

        var nowSelect = target.GetComponent<IPlayerSelectable>();
        if (nowSelect != null && !nowSelect.IsSelect)
        {
            nowSelect.OnSelect();
            ReleaseTarget();
            beforeSelected = nowSelect;
        }
        return nowSelect;
    }

    // 前回選択時のものを解除する
    public void ReleaseTarget()
    {
        if(beforeSelected == null)
        {
            Debug.Log("beforeSelected is null");
            return;
        }

        beforeSelected.OnRelease();
        beforeSelected = null;
    }
}
