using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComboTest))]
public class ComboTestEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// 元のInspector表示（comboManagerの枠など）を出す
		base.OnInspectorGUI();

		// 対象のコンポーネントを取得
		ComboTest test = (ComboTest) target;

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

		// 横並びにボタンを配置
		using(new EditorGUILayout.HorizontalScope())
		{
			if(GUILayout.Button("攻撃コンボ"))
			{
				test.WeaponCombo();
			}

			if(GUILayout.Button("アイテムコンボ(10)"))
			{
				test.AddItemCombo(10);
			}
		}

		EditorGUILayout.Space();

		// リセットボタン（色を変えて目立たせる）
		GUI.color = Color.red;
		if(GUILayout.Button("コンボのリセット"))
		{
			test.ResetCombo();
		}
		GUI.color = Color.white;

		// 実行中のみ、現在の状態を簡単に表示する（オプション）
		if(Application.isPlaying)
		{
			EditorGUILayout.HelpBox("Use these buttons to test your combo logic during Play Mode.", MessageType.Info);
		}
	}
}