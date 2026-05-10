using UnityEngine;
using UnityEngine.Events;

public class ComboManager : SingletonBehaviour<ComboManager>
{
	// --- プロパティ ---
	private Combo combo = null;
	private ComboGauge comboGauge = null;

	// --- 外部公開用のイベント ---
	public UnityAction<int> OnComboChanged; // コンボが変化した時に「現在のコンボ数」を通知する
	public event UnityAction<float> OnGaugeChanged = null; // コンボが変化した時に「現在の段階ゲージ進捗率」を通知する
	public event UnityAction<bool> OnSpecialChanged = null; // 段階ゲージ進捗率が100%の時に奥義可能フラグを通知する

	private void Awake()
	{
		// クラスの初期化
		combo = new Combo(999, 5f);
		comboGauge = new ComboGauge(300);

		// 【重要】コンボが変化した時だけゲージを更新するように紐付け（イベント駆動）
		combo.OnComboChanged += (newCount) =>
		{
			comboGauge.UpdateCombo(newCount);
			OnComboChanged?.Invoke(combo.Count);
		};

		// ゲージの計算結果をさらに外へ通知
		comboGauge.OnGaugeChanged += (val) =>
		{
			OnGaugeChanged?.Invoke(val);

			// 例：100%になったらスペシャル発動フラグを立てる
			if(val >= 100f)
				OnSpecialChanged?.Invoke(true);
		};
	}

	private void Update()
	{
		// タイマーの更新だけは毎フレーム必要
		combo.UpdateResetTime(Time.deltaTime);
	}

	// --- 各種加算メソッド ---

	// 攻撃によるコンボ加算
	public void AddWeaponCombo() => combo.AddCombo(1);
	public void AddWeaponCombo(int num) => combo.AddCombo(num);

	// アイテムによるコンボ加算
	public void AddItemCombo() => combo.AddCombo(1);
	public void AddItemCombo(int num) => combo.AddCombo(num);

	// コンボリセット
	public void ResetCombo() => combo.ResetCombo();
}