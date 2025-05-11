#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// スクリーンショット機能
public static class ScreenshotUtility 
{
	private const string outputDirectory = "Screenshots/";
	private const string fileNamePrefix = "Screenshot";

	// ctrl(macならcommand)+shift+F11をショートカットに設定
	[MenuItem("Tools/Screenshot %#F11", false)]
	public static void CaptureScreenshot() {
		// 出力先ディレクトリがなければ作成する
		if (!Directory.Exists(outputDirectory)) {
			Directory.CreateDirectory(outputDirectory);
		}

		var now = DateTime.Now;
		// 出力ファイル名を指定
		var fileName = $"{fileNamePrefix}_{now.Year}{now.Month.ToString("00")}{now.Day.ToString("00")}-{now.Hour.ToString("00")}{now.Minute.ToString("00")}{now.Second.ToString("00")}.png";
		// 出力パスを指定
		var outputPath = Path.Combine(outputDirectory, fileName);
		ScreenCapture.CaptureScreenshot(outputPath);
		// 撮影したことをログで通知
		Debug.Log($"Captured a screenshot: {fileName}");
	}
}
#endif