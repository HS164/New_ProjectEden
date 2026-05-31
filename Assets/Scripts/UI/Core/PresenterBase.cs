using UnityEngine;
using UnityEngine.UIElements;

public class PresenterBase : MonoBehaviour
{
    [SerializeField] GameObject uiPrefab;
    protected GameObject uiInstance;
    protected UIDocument uiDocument;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        uiInstance = Instantiate(uiPrefab);
        uiDocument = uiInstance.GetComponent<UIDocument>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
