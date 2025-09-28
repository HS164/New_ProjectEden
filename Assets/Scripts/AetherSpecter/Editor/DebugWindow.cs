using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugWindow : EditorWindow
{
	private static DebugWindow instance　= null;
	private VisualElement categoryContainer = null;
	private VisualElement subCategoryContainer = null;
	private VisualElement addCategoryContainer = null;

	private List<IDebugModule> cachedDebugModules;

	private enum ConfigureType
	{
		Common,
		SceneOnly
	}


	[MenuItem("Window/Debug/DebugWindow")]
	private static void ShowExample()
	{
		DebugWindow wnd = GetWindow<DebugWindow>();
		wnd.titleContent = new GUIContent("デバッグ画面");
	}

	private void CreateGUI()
	{
		if(instance ==null)
		{
			instance = this;
		}

		var root = rootVisualElement;
		root.style.flexDirection = FlexDirection.Row;

		categoryContainer = new VisualElement();
		categoryContainer.style.flexWrap = Wrap.Wrap;
		categoryContainer.style.marginLeft = 10;

		subCategoryContainer = new VisualElement();
		subCategoryContainer.style.flexWrap = Wrap.Wrap;
		subCategoryContainer.style.marginLeft = 10;

		addCategoryContainer = new VisualElement();
		addCategoryContainer.style.flexWrap = Wrap.Wrap;
		addCategoryContainer.style.marginLeft = 10;

		root.Add(ConfigureElement());
		root.Add(categoryContainer);
		root.Add(subCategoryContainer);
		root.Add(addCategoryContainer);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	private VisualElement ConfigureElement()
	{
		var element = new VisualElement();

		var commonButton = new Button(() =>
		{
			categoryContainer?.Clear();
			subCategoryContainer?.Clear();
			addCategoryContainer?.Clear();

			categoryContainer.Add(CategoryElement(ConfigureType.Common));
		})
		{
			text = "共通機能"
		};

		var sceneOnlyButton = new Button(() =>
		{
			categoryContainer?.Clear();
			subCategoryContainer?.Clear();
			addCategoryContainer?.Clear();

			categoryContainer.Add(CategoryElement(ConfigureType.SceneOnly));
		})
		{
			text = "シーン機能"
		};

		commonButton.style.width = 150;
		commonButton.style.height = 50;
		commonButton.style.marginRight = StyleKeyword.Auto;

		sceneOnlyButton.style.width = 150;
		sceneOnlyButton.style.height = 50;
		sceneOnlyButton.style.marginRight = StyleKeyword.Auto;

		element.Add(commonButton);
		element.Add(sceneOnlyButton);

		return element;
	}

	private VisualElement CategoryElement(ConfigureType configureType)
	{
		if(configureType == ConfigureType.SceneOnly)
			cachedDebugModules = DebugRegister.GetSceneOnlyModules<DebugCategoryAttribute>().ToList();
		else
			cachedDebugModules = DebugRegister.GetNonSceneOnlyModules<DebugCategoryAttribute>().ToList();

		var element = new VisualElement();

		foreach(var module in cachedDebugModules)
		{
			var categoryAttr = module.GetType().GetCustomAttribute<DebugCategoryAttribute>();
			var categoryType = categoryAttr != null ? categoryAttr.Category : default;

			var button = new Button(() =>
			{
				subCategoryContainer?.Clear();
				subCategoryContainer.Add(SubCategoryElement(categoryType));
			})
			{
				text = module.ModuleName,
				tooltip = module.Description,
			};

			button.style.width = 150;
			button.style.height = 50;
			button.style.marginRight = StyleKeyword.Auto;

			element.Add(button);
		}
		return element;
	}

	private VisualElement SubCategoryElement(CategoryType categoryType)
	{
		cachedDebugModules = DebugRegister.GetSubModulesByCategory(categoryType).ToList();

		var element = new VisualElement();

		foreach(var module in cachedDebugModules)
		{
			var button = new Button(() =>
			{
				if(!Application.isPlaying)
				{
					Debug.LogWarning("エディタ再生中ではないためはキャンセルされました");
					return;
				}
				addCategoryContainer?.Clear();
				module.Execute();
			})
			{
				text = module.ModuleName,
				tooltip = module.Description,
			};

			button.style.width = 150;
			button.style.height = 50;
			button.style.marginRight = StyleKeyword.Auto;

			element.Add(button);
		}

		return element;
	}

	public static void AddCategory(List<IDebugModule> modules)
	{
		instance.addCategoryContainer?.Clear();

		instance.cachedDebugModules = modules;

		var root = instance.rootVisualElement;
		var element = new VisualElement();

		foreach(var module in instance.cachedDebugModules)
		{
			var button = new Button(() =>
			{
				module.Execute();
				instance.cachedDebugModules.Remove(module);
			})
			{
				text = module.ModuleName,
				tooltip = module.Description,
			};

			button.style.width = 150;
			button.style.height = 50;
			button.style.marginRight = StyleKeyword.Auto;

			element.Add(button);
		}

		instance.addCategoryContainer.Add(element);
		root.Add(instance.addCategoryContainer);
	}
}
