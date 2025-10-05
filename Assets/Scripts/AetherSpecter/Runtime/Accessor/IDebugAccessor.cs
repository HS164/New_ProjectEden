using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

public interface IDebugAccessor
{
    public UIDocument SetDocument { set; }
    public DebugInputer SetInputer { set; }
    public bool IsFocused { get; set; }

    public UniTask OnInit();

    public UniTask OnUpdate();

    public UniTask OnExecute();
}
