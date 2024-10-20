using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ボタン入力
/// </summary>
public enum ActionKey
{
	None = 0,
	/// <summary>
	/// 決定
	/// </summary>
	A = 3,
	/// <summary>
	/// キャンセル
	/// </summary>
	B = 4,
	/// <summary>
	/// 
	/// </summary>
	X = 5,
	/// <summary>
	/// 
	/// </summary>
	Y = 6,
}

public class InputIndex
{
	//シングルトン
	private static InputIndex instance;
	//private static PlayerInput input;
	private const string INPUT_ACTION_ASSET_GUID_NAME = "27cc5313b6b16b04fa43e5954775fc17";

	/// <summary>
	/// 入力を使用する
	/// </summary>
	public static InputIndex Instance
	{
		get
		{
			if(instance == null)
			{
				instance = new InputIndex();
				//input = new PlayerInput();

				if(instance == null)
				{
					return null;
				}
			}
			instance.SetKeyCode();
			return instance;
		}
	}

	/// <summary>
	/// ステック入力
	/// </summary>
	private enum Stick
	{
		L,
		R,
	}


	private static InputActionAsset keyData;
	private InputActionMap actionMap;

	public async void SetKeyCode()
	{
		if(keyData != null)
		{
			return;
		}

		var data = FindGuidAssets.GetInputActionAsset(INPUT_ACTION_ASSET_GUID_NAME);
		while(data == null)
		{
			await Task.Yield();
		}

		keyData = data;
		Debug.Log(keyData.name);
		keyData.Enable();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="stick"></param>
	/// <returns></returns>
	public Vector2 LeftIsStick()
	{
		if(IsActionMap("Player") == null)
		{
			return Vector2.zero;
		}
		return actionMap.actions[Stick.L.GetHashCode()].ReadValue<Vector2>();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public Vector2 RightStick()
	{
		if(IsActionMap("Player") == null)
		{
			return Vector2.zero;
		}
		return actionMap.actions[Stick.R.GetHashCode()].ReadValue<Vector2>();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="keyCode"></param>
	/// <returns></returns>
	public bool IsKeyDown(ActionKey keyCode = ActionKey.None)
	{
		if(keyCode == ActionKey.None
		|| IsActionMap("Player") == null)
		{
			return false;
		}
		Debug.Log(actionMap.name);
		return actionMap.actions[keyCode.GetHashCode()].IsPressed();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="map_name"></param>
	/// <returns></returns>
	private InputActionMap IsActionMap(string map_name)
	{
		if(actionMap != null)
		{
			return actionMap;
		}
		return actionMap = keyData.actionMaps[0];
	}
}
