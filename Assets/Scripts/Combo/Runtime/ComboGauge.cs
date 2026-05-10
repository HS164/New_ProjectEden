using UnityEngine;
using UnityEngine.Events;

public class ComboGauge
{
	private int maxComboCount = 300;

	// --- プロパティ ---
	public float Value { get; private set; } = 0;

	/// <summary>
	/// ゲージの進捗率が変更された時の通知 (0 ~ 100)
	/// </summary>
	public event UnityAction<float> OnGaugeChanged;
	
	public ComboGauge(int maxCombo)
	{
		// 0除算防止のため、最低1以上に制限
		maxComboCount = Mathf.Max(1, maxCombo);
	}

	/// <summary>
	/// 現在のコンボ数を受け取り、ゲージの％を更新する
	/// </summary>
	public void UpdateCombo(int currentCombo)
	{
		// (float)でキャストして、小数点以下の計算を有効にする
		float ratio = Mathf.Clamp01((float) currentCombo / maxComboCount);
		float newValue = ratio * 100f;

		if(!Mathf.Approximately(Value, newValue))
		{
			Value = newValue;
			OnGaugeChanged?.Invoke(Value);
		}
	}
}