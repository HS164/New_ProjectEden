using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugCategoryComponent : MonoBehaviour, IDebugAccessor
{
    private UIDocument uiDocument = null;

    public UIDocument SetDocument { set => uiDocument = value; }
    public Debuginputer SetInputer { set; private get; }
    public bool IsFocused { get; set; }

    public UniTask OnInit()
    {
        return UniTask.CompletedTask;
    }

    public UniTask OnUpdate()
    {
        return UniTask.CompletedTask;
    }

    public UniTask OnExecute()
    {
        return UniTask.CompletedTask;
    }
}
