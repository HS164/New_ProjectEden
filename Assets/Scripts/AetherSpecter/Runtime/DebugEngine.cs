using UnityEngine;
using UnityEngine.UIElements;

public class DebugEngine : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument = null;

    private Debuginputer Inputer = null;
    private IDebugAccessor accessor = null;

    private void Awake()
    {
        Inputer = new();

        accessor = GetComponent<IDebugAccessor>();

        if (accessor != null)
        {
            accessor.OnInit();
        }
        else return;

        accessor.SetDocument = uiDocument;
        accessor.SetInputer = Inputer;

        uiDocument.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (accessor != null)
        {
            accessor.OnUpdate();
        }
    }

    private void OnEnable() => Inputer.DebugMenu.Enable();
    private void OnDisable() => Inputer.DebugMenu.Disable();
    private void OnDestroy() => Inputer.Dispose();
}
