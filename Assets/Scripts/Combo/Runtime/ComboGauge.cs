using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// コンボ数に応じて進捗率（0.0 ~ 1.0）を管理し、特定の閾値でイベントを発火させるクラス
/// </summary>
public class ComboGauge
{
	public float THRESHOLD_STEP1 { get; private set; } = 0.25f; // 第1段階: 25%
	public float THRESHOLD_STEP2 { get; private set; } = 0.50f; // 第2段階: 50%
	public float THRESHOLD_STEP3 { get; private set; } = 0.75f; // 第3段階: 75%

	private const int StepNone = 0;
	private const int Step1 = 1;
	private const int Step2 = 2;
	private const int Step3 = 3;

	// --- フィールド ---
	private readonly int _maxComboCount;

	/// <summary>
	/// 最後に通知した段階を記録 (0: 未到達, 1: 25%超, 2: 50%超, 3: 75%超)
	/// </summary>
	private int _lastNotifiedStep = StepNone;

	// --- プロパティ ---

	/// <summary>
	/// 現在の進捗率 (0.0 ~ 1.0)
	/// </summary>
	public float Progress { get; private set; } = 0f;

	/// <summary>
	/// ゲージの進捗率が変更された時の通知
	/// </summary>
	public event UnityAction<float> OnProgressChanged;

	/// <summary>
	/// 特定の段階（1, 2, 3段階目）に達した時の通知
	/// </summary>
	public event UnityAction<int> OnThresholdReached;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="maxCombo">最大コンボ数</param>
	public ComboGauge(int maxCombo)
	{
		// 0除算防止のため、最低1以上に制限
		_maxComboCount = Mathf.Max(1, maxCombo);
	}

	/// <summary>
	/// 現在のコンボ数を受け取り、進捗率と通知状況を更新する
	/// </summary>
	/// <param name="currentCombo">現在の累計コンボ数</param>
	public void UpdateCombo(int currentCombo)
	{
		// 現在のコンボ率を 0.0 ~ 1.0 の範囲で算出
		float newProgress = Mathf.Clamp01((float) currentCombo / _maxComboCount);

		// 値に変化があった場合のみ処理
		if(!Mathf.Approximately(Progress, newProgress))
		{
			Progress = newProgress;
			OnProgressChanged?.Invoke(Progress);

			// 閾値チェック
			CheckThresholds(Progress);
		}
	}

	/// <summary>
	/// 現在の進捗率に基づき、段階的な通知イベントを発火させる
	/// </summary>
	/// <param name="currentProgress">現在の進捗 (0.0 ~ 1.0)</param>
	private void CheckThresholds(float currentProgress)
	{
		int currentStep = StepNone;

		// 現在どのフェーズにいるか判定
		if(currentProgress >= THRESHOLD_STEP3)
			currentStep = Step3;
		else if(currentProgress >= THRESHOLD_STEP2)
			currentStep = Step2;
		else if(currentProgress >= THRESHOLD_STEP1)
			currentStep = Step1;

		// 前回の通知ステップより進んだ場合のみイベント発行
		if(currentStep > _lastNotifiedStep)
		{
			_lastNotifiedStep = currentStep;
			OnThresholdReached?.Invoke(_lastNotifiedStep);
		}
		// コンボが途切れた（リセットされた）場合に通知状況もリセット
		else if(currentProgress <= 0f)
		{
			_lastNotifiedStep = StepNone;
		}
	}
}