using UnityEditor;
using UnityEngine.InputSystem;

public class FindGuidAssets
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static InputActionAsset GetInputActionAsset(string guid_name)
	{
		string asset_path = AssetDatabase.GUIDToAssetPath(guid_name);
		var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(asset_path);
		return inputActions;
	}
}
