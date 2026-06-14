using UnityEngine;
using UnityEngine.UIElements;

public class ComboGaugeUIPresenter : PresenterBase
{
	private VisualElement hpBarFill;
	private VisualElement step1Line;
	private VisualElement step2Line;
	private VisualElement step3Line;
	protected override void Start()
	{
		base.Start();

		hpBarFill = uiDocument.rootVisualElement.Q<VisualElement>("HpBarFill");
		step1Line = uiDocument.rootVisualElement.Q<VisualElement>("Step1Line");
		step2Line = uiDocument.rootVisualElement.Q<VisualElement>("Step2Line");
		step3Line = uiDocument.rootVisualElement.Q<VisualElement>("Step3Line");

		BindToComboManager();
	}

	private void BindToComboManager()
	{
		ComboManager comboManager = ComboManager.Instance;
		comboManager.OnGaugeChanged += OnGaugeChanged;
		SetStepLinePos(step1Line, comboManager.GetThresholdStep1());
		SetStepLinePos(step2Line, comboManager.GetThresholdStep2());
		SetStepLinePos(step3Line, comboManager.GetThresholdStep3());
	}

	private void OnGaugeChanged(float gaugePercent)
	{
		hpBarFill.style.width = Length.Percent(gaugePercent * 100f);
	}

	private void SetStepLinePos(VisualElement stepLine, float linePercent)
	{
		stepLine.style.left = Length.Percent(linePercent * 100f);
	}
}
