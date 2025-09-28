using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

public interface IDebugAccessor
{
    public UIDocument SetDocument { set; }
    public Debuginputer SetInputer { set; }
    public bool IsFocused { get; set; }

    public UniTask OnInit();

    public UniTask OnUpdate();

    public UniTask OnExecute();
}
