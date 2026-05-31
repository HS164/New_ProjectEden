using UnityEngine;
using UnityEngine.Events;

public class Combo
{
	// --- プロパティ ---
	public int Count { get; private set; } = 0;
	public int MaxCount { get; private set; } = 300;

	// タイマーの進捗率 (0.0 ~ 1.0) を外から参照できるようにするとUIバーの実装が楽になります
	public float ResetTimerNormalized => Mathf.Clamp01(currentTimer / resetTime);

	// --- イベント ---
	// コンボが変化した時に「現在のコンボ数」を通知する
	public UnityAction<int> OnComboChanged;
	// コンボがリセットされた時に通知する
	public UnityAction OnComboReset;

	private float resetTime = 5.0f;
	private float currentTimer = 0;
	private bool isCounting = false;

	public Combo(int maxCombo, float resetTime)
	{
		this.MaxCount = maxCombo;
		this.resetTime = resetTime;
	}

	/// <summary>
	/// Updateから呼び出し。タイマーが動いている時だけ計算。
	/// </summary>
	public void UpdateResetTime(float deltaTime)
	{
		if(!isCounting)
			return;

		currentTimer -= deltaTime;
		if(currentTimer <= 0)
		{
			ResetCombo();
		}
	}

	/// <summary>
	/// コンボ加算の共通処理
	/// </summary>
	public void AddCombo(int num = 1)
	{
		int prevCount = Count;
		Count = Mathf.Clamp(Count + num, 0, MaxCount);

		// 値が変わった時だけイベントを呼ぶ
		if(prevCount != Count)
		{
			OnComboChanged?.Invoke(Count);
		}

		StartResetTimer();
	}

	/// <summary>
	/// コンボリセット
	/// </summary>
	public void ResetCombo()
	{
		if(Count == 0)
			return; // 既に0なら何もしない

		Count = 0;
		isCounting = false;
		currentTimer = 0;
		OnComboReset?.Invoke();
		OnComboChanged?.Invoke(Count);
	}

	private void StartResetTimer()
	{
		currentTimer = resetTime;
		isCounting = true;
	}
}