using UnityEngine;
using UnityEngine.UIElements;

public class DebugEngine : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument = null;

    private DebugInputer inputer = null;
    private IDebugAccessor accessor = null;

    private void Awake()
    {
        inputer = new();

        accessor = GetComponent<IDebugAccessor>();

        if (accessor != null)
        {
            accessor.OnInit();
        }
        else return;

        accessor.SetDocument = uiDocument;
        accessor.SetInputer = inputer;

        uiDocument.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (accessor != null)
        {
            accessor.OnUpdate();
        }
    }

    private void OnEnable() => inputer.DebugMenu.Enable();
    private void OnDisable() => inputer.DebugMenu.Disable();
    private void OnDestroy() => inputer.Dispose();
}
