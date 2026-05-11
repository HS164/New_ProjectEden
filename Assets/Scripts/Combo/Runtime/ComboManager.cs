using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// コンボシステム全体を統括するマネージャークラス。
/// コンボカウントの増減と、それに伴うゲージ段階のイベント発行を管理する。
/// </summary>
public class ComboManager : SingletonBehaviour<ComboManager>
{
	// --- 設定定数 ---
	private const int MAX_COMBO_LIMIT = 999;      // コンボカウントの最大値
	private const float COMBO_EXPIRE_TIME = 5f;  // コンボが途切れるまでの時間
	private const int GAUGE_MAX_COMBO = 300;     // ゲージが100%に達するコンボ数
	private const float FULL_GAUGE_THRESHOLD = 1.0f; // ゲージ満タン(100%)の閾値

	// --- 内部コンポーネント ---
	private Combo _combo = null;
	private ComboGauge _comboGauge = null;

	// --- 外部公開用のイベント ---

	/// <summary>コンボ数が変化した際の通知 (引数: 現在のコンボ数)</summary>
	public event UnityAction<int> OnComboChanged = null;

	/// <summary>ゲージの進捗率が変化した際の通知 (引数: 0.0 ~ 1.0)</summary>
	public event UnityAction<float> OnGaugeChanged = null;

	/// <summary>進捗が第1段階(25%)に到達し「疾風」が使用可能になった通知</summary>
	public event UnityAction OnGaleChanged = null;

	/// <summary>進捗が第2段階(50%)に到達し「迅雷」が使用可能になった通知</summary>
	public event UnityAction OnBlitzChanged = null;

	/// <summary>進捗が第3段階(75%)に到達し「神速」が使用可能になった通知</summary>
	public event UnityAction OnHyperdriveChanged = null;

	/// <summary>進捗が100%に到達し「奥義」が使用可能になった通知</summary>
	public event UnityAction OnSpecialChanged = null;

	protected override void Awake()
	{
		base.Awake(); // SingletonBehaviourの初期化

		// 各クラスのインスタンス化
		_combo = new Combo(MAX_COMBO_LIMIT, COMBO_EXPIRE_TIME);
		_comboGauge = new ComboGauge(GAUGE_MAX_COMBO);

		// コンボ数の変化を監視し、ゲージへ反映させる
		_combo.OnComboChanged += (newCount) =>
		{
			_comboGauge.UpdateCombo(newCount);
			OnComboChanged?.Invoke(newCount);
		};

		// ゲージ進捗の変化を監視
		_comboGauge.OnProgressChanged += (progress) =>
		{
			OnGaugeChanged?.Invoke(progress);

			// 1.0(100%)到達時の判定
			if(progress >= FULL_GAUGE_THRESHOLD)
			{
				OnSpecialChanged?.Invoke();
			}
		};

		// ゲージの段階到達(閾値越え)を監視
		_comboGauge.OnThresholdReached += (step) =>
		{
			switch(step)
			{
				case 1: // 25%到達
					OnGaleChanged?.Invoke();
					break;
				case 2: // 50%到達
					OnBlitzChanged?.Invoke();
					break;
				case 3: // 75%到達
					OnHyperdriveChanged?.Invoke();
					break;
			}
		};
	}

	private void Update()
	{
		// コンボ維持時間のカウントダウン更新
		_combo.UpdateResetTime(Time.deltaTime);
	}

	// --- コンボ操作用公開メソッド ---

	/// <summary>攻撃によるコンボ加算 (デフォルト1)</summary>
	public void AddWeaponCombo(int num = 1) => _combo.AddCombo(num);

	/// <summary>アイテムによるコンボ加算 (デフォルト1)</summary>
	public void AddItemCombo(int num = 1) => _combo.AddCombo(num);

	/// <summary>コンボの強制リセット</summary>
	public void ResetCombo() => _combo.ResetCombo();
}