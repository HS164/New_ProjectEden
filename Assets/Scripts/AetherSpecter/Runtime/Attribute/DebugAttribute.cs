using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class SceneDebugOnlyAttribute : Attribute
{
	//public SceneName SceneFlags
	//{
	//	get; private set;
	//}

	//public SceneDebugOnlyAttribute(SceneName sceneFlags)
	//{
	//	SceneFlags = sceneFlags;
	//}

	///// <summary>
	///// åªç›ÉVÅ[ÉìÇ…ä‹Ç‹ÇÍÇÈÇ©Ç«Ç§Ç©îªíË
	///// </summary>
	//public bool Contains(SceneName currentScene)
	//{
	//	return (SceneFlags & currentScene) != 0;
	//}
}

[AttributeUsage(AttributeTargets.Class)]
public class DebugCategoryAttribute : Attribute
{
	public CategoryType Category
	{
		get; private set;
	}

	public DebugCategoryAttribute(CategoryType category)
	{
		Category = category;
	}
}

[AttributeUsage(AttributeTargets.Class)]
public class DebugSubCategoryAttribute : Attribute
{
	public CategoryType Category
	{
		get; private set;
	}

	public DebugSubCategoryAttribute(CategoryType category)
	{
		Category = category;
	}
}
