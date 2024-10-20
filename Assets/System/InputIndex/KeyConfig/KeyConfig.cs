using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyConfig : MonoBehaviour
{
	[SerializeField] Image backGround = null;

	private void Awake()
	{
		SettingScaler();
	}

	private void SettingScaler()
	{
		var rect = backGround.rectTransform.sizeDelta;
		rect.x = 1920;
		rect.y = 1080;
		backGround.rectTransform.sizeDelta = rect;
	}
}
