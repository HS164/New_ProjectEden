using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[DebugCategory(CategoryType.Character)]
public class ObjectCreateModule : IDebugModule
{
    public string ModuleID => "";
    public string ModuleName => "生成";
    public string Description => "オブジェクトの自動生成";

    public UniTask Execute() => UniTask.CompletedTask;
}

//[DebugSubCategory(CategoryType.Character)]
//public class Item : IDebugModule
//{
//    public string ModuleID => "ObjectCreateModule_000";
//    public string ModuleName => "Item";
//    public string Description => "Item";

//    public async UniTask Execute()
//    {
//        await Addressables.InstantiateAsync("Assets/Prefabs/Player/Obj.prefab", Vector3.one, Quaternion.identity);
//    }
//}

[DebugSubCategory(CategoryType.Character)]
public class Obj : IDebugModule
{
    public string ModuleID => "ObjectCreateModule_001";
    public string ModuleName => "Object";
    public string Description => "Object";

    public async UniTask Execute()
    {
        await Addressables.InstantiateAsync("Assets/Prefabs/Player/ObjSet.prefab", Vector3.one, Quaternion.identity);
    }
}

