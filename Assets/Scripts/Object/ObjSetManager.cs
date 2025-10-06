using UnityEngine;
using Cysharp.Threading.Tasks;

public class ObjSetManager : MonoBehaviour
{
    private void Start()
    {
        Init().Forget();
    }

    private async UniTask Init()
    {
        foreach(Transform child in transform)
        {
            Debug.Log(child.name);
            child.gameObject.SetActive(true);
            await UniTask.WaitForSeconds(0.1f);
        }
    }
}
