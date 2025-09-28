using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugChatComponent : MonoBehaviour, IDebugAccessor
{
    [SerializeField]
    private string TextField_Name = "TextField";
    [SerializeField]
    private string Button_Name = "CmdEnter";

    private UIDocument uiDocument = null;
    private TextField textField = null;
    private Button enterButton = null;
    private string[] commandParts = null; // コマンドを空白で分割した配列
    private string commandInput = null; // 入力されたコマンド文字列

    // キャッシュ用
    private List<IDebugModule> cachedDebugModules;
    private List<IDebugModule> cachedSubModules;

    public UIDocument SetDocument { set => uiDocument = value; } // UIDocument をセット
    public Debuginputer SetInputer { get; set; } // 入力を受け取る Debuginputer
    public bool IsFocused { get; set; } // テキストフィールドがフォーカスされているか

    /// <summary>
    /// 初期化処理。必要があれば追加の初期化を行う。
    /// </summary>
    public async UniTask OnInit()
    {
        await LoadAsyncModules();
    }

    /// <summary>
    /// 毎フレーム呼ばれる更新処理。
    /// デバッグメニューの開閉キーを検知し、UI表示の切り替えを行う。
    /// </summary>
    public UniTask OnUpdate()
    {
        if (SetInputer.DebugMenu.Open.WasPressedThisFrame() && !IsFocused)
        {
            bool isActive = uiDocument.gameObject.activeSelf;
            uiDocument.gameObject.SetActive(!isActive);

            if (!isActive)
            {
                SetupTextFieldCallbacks();
                SetupEnterButton();
            }
        }

        if (SetInputer.DebugMenu.Close.WasPressedThisFrame())
        {
            uiDocument.gameObject.SetActive(false);
            ReleasedModules();
            IsFocused = false;
        }
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 入力されたコマンドを解析して対応するデバッグモジュールを実行する。
    /// </summary>
    public UniTask OnExecute()
    {
        // 入力コマンドに対応するメインモジュール
        IDebugModule mainModule = cachedDebugModules.FirstOrDefault(
            m => string.Equals(m.ModuleName, commandParts[0], StringComparison.OrdinalIgnoreCase));

        if (mainModule == null) return UniTask.CompletedTask;

        // サブモジュール検索
        IDebugModule subModule = cachedSubModules.FirstOrDefault(
            m => string.Equals(m.ModuleName, commandParts[1], StringComparison.OrdinalIgnoreCase));

        if (subModule == null) return UniTask.CompletedTask;

        subModule.Execute();
        textField.value = "";
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// テキストフィールドのフォーカス状態および入力値の変更を監視するコールバックを設定する。
    /// </summary>
    public UniTask SetupTextFieldCallbacks()
    {
		textField = uiDocument.rootVisualElement.Q<TextField>(TextField_Name);
		if(textField == null)
		{
			Debug.LogError("TextField がまだ存在しません");
			return UniTask.CompletedTask;
		}

		textField.schedule.Execute(() =>
		{
			textField.Focus();
		}).StartingIn(0);

		textField.RegisterValueChangedCallback(evt =>
		{
			commandInput = evt.newValue;
		});

		textField.RegisterCallback<KeyDownEvent>(evt =>
		{
			if(evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
			{
				// 最新の入力値を即時取得（text を使う）
				commandInput = textField.text;

				// ①「/」から始まらない場合は無視
				if(!commandInput.StartsWith("/"))
					return;

				// ②「/」を除いてコマンド名と引数を分離
				string[] parts = commandInput.Substring(1)
					.Split(' ', StringSplitOptions.RemoveEmptyEntries);

				if(parts.Length > 0)
				{
					string commandName = parts[0];
					string[] args = parts.Skip(1).ToArray();

					// ここで commandName / args を使って実行する
					commandParts = parts; // 必要なら保持
				}

				// コマンド実行
				OnExecute().Forget();

				// 入力欄をクリア（マイクラ風）
				textField.value = string.Empty;

				// Enter の既定動作（改行）を止める
				evt.StopPropagation();
			}
		});

		textField.RegisterCallback<FocusInEvent>(evt => IsFocused = true);
		textField.RegisterCallback<FocusOutEvent>(evt => IsFocused = false);
		return UniTask.CompletedTask;
    }

	/// <summary>
	/// Enterボタンが押されたときのコマンド実行処理を設定する。
	/// </summary>
	public UniTask SetupEnterButton()
    {
        enterButton = uiDocument.rootVisualElement.Q<Button>(Button_Name);
        if (enterButton == null)
        {
            Debug.LogError("CmdEnter がまだ存在しません");
            return UniTask.CompletedTask;
        }

        enterButton.RegisterCallback<PointerUpEvent>(evt =>
        {
            commandParts = commandInput.Split(' ');
            OnExecute().Forget();
        });
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 全アセンブリからモジュールを読み込み
    /// </summary>
    /// <returns></returns>
    private UniTask LoadAsyncModules()
    {
        if (cachedDebugModules == null)
        {
            cachedDebugModules = DebugRegister.GetModules<DebugCategoryAttribute>().ToList();
        }

        if (cachedSubModules == null)
        {
            cachedSubModules = DebugRegister.GetModules<DebugSubCategoryAttribute>().ToList();
        }
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// モジュールの解放
    /// </summary>
    private void ReleasedModules()
    {
        cachedDebugModules = null;
        cachedSubModules = null;
    }
}
