using UnityEngine;

// テスト用のコンポーネント
public class ComboTest : MonoBehaviour
{
	[SerializeField] private ComboManager comboManager;
	[SerializeField] private int comboCount = 0;

	private void Start()
	{
		comboManager.OnComboChanged += (num) => comboCount = num;
		comboManager.OnGaugeChanged += (num) => Debug.Log($"段階ゲージ | {num}%");
		comboManager.OnSpecialChanged += (flag) =>   Debug.Log("奥義使用可能!!");
	}

	// Editorから呼び出すためのメソッド
	public void WeaponCombo() => comboManager.AddWeaponCombo();
	public void AddItemCombo(int amount) => comboManager.AddItemCombo(amount);
	public void ResetCombo() => comboManager.ResetCombo();
}