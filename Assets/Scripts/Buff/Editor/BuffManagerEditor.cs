using UnityEditor;
using UnityEngine;

/// <summary>
/// BuffManagerのインスペクター拡張。
/// PlayMode中にAddressablesからロードされたバフ定義を確認専用で表示する。
/// </summary>
[CustomEditor(typeof(BuffManager))]
public class BuffManagerEditor : Editor
{
    private bool _speedBuffFoldout = true;

    public override void OnInspectorGUI()
    {
        var manager = (BuffManager)target;

        // デフォルトのインスペクター
        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("ロード済みバフ定義", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("PlayMode中のみ表示されます。", MessageType.Info);
            return;
        }

        // ロード状態の表示
        var statusColor = manager.IsReady ? Color.green : Color.yellow;
        var statusText = manager.IsReady ? "ロード完了" : "ロード中...";
        using (new GUIColorScope(statusColor))
        {
            EditorGUILayout.LabelField("ステータス", statusText);
        }

        if (!manager.IsReady)
        {
            return;
        }

        EditorGUILayout.Space(4);

        // 速度バフ一覧
        DrawSpeedBuffList(manager.Speed);
    }

    private void DrawSpeedBuffList(SpeedBuffSystem speedSystem)
    {
        var data = speedSystem.LoadedData;
        _speedBuffFoldout = EditorGUILayout.Foldout(_speedBuffFoldout, $"速度バフ / デバフ  ({data.Count}件)", true);

        if (!_speedBuffFoldout)
        {
            return;
        }

        if (data.Count == 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("データなし");
            EditorGUI.indentLevel--;
            return;
        }

        using var _ = new EditorGUI.IndentLevelScope();
        foreach (var kv in data)
        {
            DrawSpeedBuffRow(kv.Value);
        }
    }

    private void DrawSpeedBuffRow(SpeedBuffData buffData)
    {
        using var box = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField("BuffId", buffData.BuffId);
            EditorGUILayout.FloatField("Multiplier", buffData.Multiplier);
            EditorGUILayout.Toggle("IsStackable", buffData.IsStackable);
        }
    }

    // Guiの色を一時的に変えるスコープヘルパー
    private readonly struct GUIColorScope : System.IDisposable
    {
        private readonly Color _previous;

        public GUIColorScope(Color color)
        {
            _previous = GUI.color;
            GUI.color = color;
        }

        public void Dispose() => GUI.color = _previous;
    }
}
