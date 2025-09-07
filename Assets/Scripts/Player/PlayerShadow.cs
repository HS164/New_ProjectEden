using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShadow : MonoBehaviour
{
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private float shadowLifetime;

    void Start()
    {
        ShadowDecay().Forget();
    }

    /// <summary>
    /// 残像の色を変化する、終わったらオブジェクト破壊
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid ShadowDecay()
    {
        
        MeshRenderer mesh = GetComponent<MeshRenderer>();

        mesh.material.color = startColor;

        float elapsed = 0f;
        while (elapsed < shadowLifetime)
        {
            elapsed += Time.deltaTime;
            mesh.material.color = Color.Lerp(startColor, endColor, elapsed / shadowLifetime);
            await UniTask.Yield();
        }

        Destroy(gameObject);
    }
}
