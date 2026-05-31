using UnityEngine;
using UnityEngine.UIElements;

public class ComboCounterPresenter : PresenterBase
{
	private Label labelElement;
	protected override void Start()
	{
		base.Start();

		labelElement = uiDocument.rootVisualElement.Q<Label>("ComboLabel");

		BindToComboManager();
	}

	private void BindToComboManager()
	{
		ComboManager.Instance.OnComboChanged += OnComboUpdated;
	}

	private void OnComboUpdated(int combo)
	{
		labelElement.text = GetComboText(combo);
	}

	private string GetComboText(int combo)
	{
		return combo.ToString("d3") + "Combo";

	}
}
