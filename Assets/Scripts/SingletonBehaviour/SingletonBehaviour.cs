using UnityEngine;

/// <summary>
/// シングルトンパターン用のBehaviourを作成するクラス
/// </summary>
/// <typeparam name="T">シングルトンにしたいクラス名</typeparam>
public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    // シングルトンインスタンス
    private static T instance;

    // シングルトンインスタンス
    public static T Instance
    {
        get => instance;
        protected set
        {
            // インスタンスがまだ存在しない場合、このインスタンスをシングルトンにする
            if (instance == null)
            {
                instance = value;
                // シーンを切り替えてもこのオブジェクトを破棄しないようにする
                DontDestroyOnLoad(instance.transform.gameObject);
            }
            else
            {
                // 既にインスタンスが存在する場合、このオブジェクトを破棄する
                Destroy(instance.transform.gameObject);
            }
        }
    }

    protected virtual void Awake()
    {
        Instance = this as T;
    }
}
