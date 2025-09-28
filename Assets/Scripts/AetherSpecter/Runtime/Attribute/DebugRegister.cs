using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DebugRegister
{
	/// <summary>
	/// 指定属性を持つ IDebugModule を全アセンブリから取得する汎用関数。
	/// </summary>
	/// <typeparam name="TAttribute">検索対象の属性型</typeparam>
	/// <returns>属性を持つ IDebugModule の列挙</returns>
	public static IEnumerable<IDebugModule> GetModules<TAttribute>() where TAttribute : Attribute
	{
		return AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.Where(t => t.GetCustomAttribute<TAttribute>() != null) // 指定属性を持つ型のみ
			.Where(t => typeof(IDebugModule).IsAssignableFrom(t))   // IDebugModule を継承しているか
			.Select(t => Activator.CreateInstance(t) as IDebugModule) // インスタンス化
			.Where(m => m != null); // null は除外
	}

	/// <summary>
	/// SceneDebugOnlyAttribute が付与されているDebugModule を検索
	/// </summary>
	public static IEnumerable<IDebugModule> GetSceneOnlyModules<TAttribute>() where TAttribute : Attribute
	{
		return GetModules<TAttribute>()
			.Where(m => m.GetType().GetCustomAttribute<SceneDebugOnlyAttribute>() != null);
	}

	/// <summary>
	/// SceneDebugOnlyAttribute が付与されていない IDebugModule を検索
	/// </summary>
	public static IEnumerable<IDebugModule> GetNonSceneOnlyModules<TAttribute>() where TAttribute : Attribute
	{
		return GetModules<TAttribute>()
			.Where(m => m.GetType().GetCustomAttribute<SceneDebugOnlyAttribute>() == null);
	}

	/// <summary>
	/// 指定属性を持つ IDebugModule のうち、特定 CategoryType のものだけ返す
	/// </summary>
	public static IEnumerable<IDebugModule> GetSubModulesByCategory(CategoryType category)
	{
		return GetModules<DebugSubCategoryAttribute>()  // まずは汎用関数で属性付きモジュール取得
			.Where(m =>
			{
				var attr = m.GetType().GetCustomAttribute<DebugSubCategoryAttribute>();
				return attr != null && attr.Category == category;
			});
	}
}
