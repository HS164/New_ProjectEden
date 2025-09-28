using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
public class DebugModuleGenerater : EditorWindow
{
    private TextField m_NameField;
    private TextField m_FolderPathField;
    private EnumField m_CategoryField;
    private TextField m_CategoryDescriptionField;
    private VisualElement m_SubCategoryListContainer;

    private SubCategoryManager subCategoryManager = new SubCategoryManager();
    private const string PrefKey_LastFolder = "DebugModuleGenerater_LastFolder";

    [MenuItem("Window/Debug/DebugModuleGenerater")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<DebugModuleGenerater>();
        wnd.titleContent = new GUIContent("デバッグモジュール生成ツール");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;

        // 保存先パスUI
        root.Add(CreateFolderSelectionRow());

        // クラス名UI
        m_NameField = new TextField("クラス名");
        root.Add(m_NameField);

        // カテゴリUI
        m_CategoryField = new EnumField("カテゴリ", (CategoryType)0);
        root.Add(m_CategoryField);

        // カテゴリ説明 TextField
        m_CategoryDescriptionField = new TextField("カテゴリ説明");
        m_CategoryDescriptionField.style.marginBottom = 5;
        m_CategoryDescriptionField.isReadOnly = false; // 編集可能
        root.Add(m_CategoryDescriptionField);

        // サブカテゴリ入力UI
        root.Add(CreateSubCategoryInputRow());

        // サブカテゴリ表示コンテナ
        m_SubCategoryListContainer = new VisualElement();
        m_SubCategoryListContainer.style.flexDirection = FlexDirection.Column;
        m_SubCategoryListContainer.style.marginTop = 5;
        root.Add(m_SubCategoryListContainer);
        RefreshSubCategoryList();

        // 生成ボタン
        root.Add(CreateGenerateButton());
    }

    #region UI生成メソッド

    private VisualElement CreateFolderSelectionRow()
    {
        var folderRow = new VisualElement();
        folderRow.style.flexDirection = FlexDirection.Row;
        folderRow.style.alignItems = Align.Center;

        string defaultFolder = EditorPrefs.GetString(PrefKey_LastFolder, "Assets");
        m_FolderPathField = new TextField("保存先パス");
        m_FolderPathField.value = defaultFolder;
        m_FolderPathField.isReadOnly = true;
        m_FolderPathField.style.flexGrow = 1;
        folderRow.Add(m_FolderPathField);

        var selectFolderButton = new Button(() =>
        {
            string selectedPath = EditorUtility.OpenFolderPanel("スクリプトの保存先を選択", m_FolderPathField.value, "");
            if (string.IsNullOrEmpty(selectedPath)) return;

            selectedPath = selectedPath.Replace("\\", "/");

            if (selectedPath.StartsWith(Application.dataPath))
            {
                string relativePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                m_FolderPathField.value = relativePath;
                EditorPrefs.SetString(PrefKey_LastFolder, relativePath);
            }
            else
            {
                Debug.LogError("エラー: 選択したフォルダはプロジェクトの Assets 内にしてください。");
            }
        })
        { text = "参照..." };
        selectFolderButton.style.width = 100;
        selectFolderButton.style.marginLeft = StyleKeyword.Auto;
        folderRow.Add(selectFolderButton);

        return folderRow;
    }

    private VisualElement CreateSubCategoryInputRow()
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;

        var label = new Label("サブカテゴリ名");
        label.style.width = 100;
        row.Add(label);

        var inputField = new TextField();
        inputField.style.flexGrow = 1;
        row.Add(inputField);

        var addButton = new Button(() =>
        {
            string value = inputField.value.Trim();
            if (!string.IsNullOrEmpty(value))
            {
                subCategoryManager.Add(value);
                inputField.value = "";
                RefreshSubCategoryList();
            }
        })
        { text = "追加" };
        addButton.style.width = 100;
        addButton.style.marginLeft = StyleKeyword.Auto;
        row.Add(addButton);

        return row;
    }

    private Button CreateGenerateButton()
    {
        return new Button(() =>
        {
            string moduleName = m_NameField.value;
            if (string.IsNullOrEmpty(moduleName))
            {
                Debug.LogError("エラー: モジュール名が入力されていません。");
                return;
            }

            string folderPath = m_FolderPathField.value;
            var category = (CategoryType)m_CategoryField.value;
            string categoryDescription = m_CategoryDescriptionField.value;

            CreateModuleScript(moduleName, folderPath, category, categoryDescription);

            // 入力欄とリストを初期化
            ResetWindow();
        })
        { text = "スクリプトを生成" };
    }

    #endregion

    #region サブカテゴリ管理

    private void RefreshSubCategoryList()
    {
        m_SubCategoryListContainer.Clear();
        var allSubs = subCategoryManager.GetAll();
        for (int i = 0; i < allSubs.Count; i++)
        {
            string sub = allSubs[i];
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 2;

            // 左側ラベルを固定幅で左揃え
            var label = new Label(sub);
            label.style.width = 100;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            row.Add(label);

            // 空の要素でスペースを確保
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            row.Add(spacer);

            int index = i;
            var deleteButton = new Button(() =>
            {
                subCategoryManager.RemoveAt(index);
                RefreshSubCategoryList();
            })
            { text = "削除" };
            deleteButton.style.width = 100;
            row.Add(deleteButton);

            m_SubCategoryListContainer.Add(row);
        }
    }

    #endregion

    #region 初期化

    private void ResetWindow()
    {
        m_NameField.value = "";
        m_CategoryField.value = (CategoryType)0;
        m_CategoryDescriptionField.value = "";
        subCategoryManager.Clear();
        RefreshSubCategoryList();
    }

    #endregion

    #region スクリプト生成

    private void CreateModuleScript(string moduleName, string folderPath, CategoryType category, string categoryDescription)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("出力先フォルダを作成しました: " + folderPath);
        }

        string scriptPath = Path.Combine(folderPath, moduleName + ".cs");
        if (File.Exists(scriptPath))
        {
            Debug.LogError("エラー: 同名のスクリプトが存在します → " + scriptPath);
            return;
        }

        string template = GenerateScriptTemplate(moduleName, category, categoryDescription, subCategoryManager.GetAll());
        File.WriteAllText(scriptPath, template);
        AssetDatabase.Refresh();
        Debug.Log($"スクリプトを生成しました: {scriptPath}");
    }

    private string GenerateScriptTemplate(string moduleName, CategoryType category, string categoryDescription, List<string> subCategories)
    {
        string template = GenerateParentModuleTemplate(moduleName, category, categoryDescription);
        for (int i = 0; i < subCategories.Count; i++)
            template += GenerateSubModuleTemplate(moduleName, i, subCategories[i], category);
        return template;
    }

    private string GenerateParentModuleTemplate(string moduleName, CategoryType category, string categoryDescription)
    {
        return
$@"using Cysharp.Threading.Tasks;
using UnityEngine;

[DebugCategory(CategoryType.{category})]
public class {moduleName} : IDebugModule
{{
    public override string ModuleID => ""{moduleName}"";
    public override string ModuleName => ""{moduleName}"";
    public override string Description => ""{categoryDescription}"";

    public UniTask Execute() => UniTask.CompletedTask;
}}

";
    }

    private string GenerateSubModuleTemplate(string moduleName, int index, string subName, CategoryType category)
    {
        return
$@"[DebugSubCategory(CategoryType.{category})]
public class {subName} : IDebugModule
{{
    public string ModuleID => ""{moduleName}_{index:D3}"";
    public string ModuleName => ""{subName}"";
    public string Description => ""{subName}"";

    public async UniTask Execute()
    {{
        Debug.Log(Description);
        // 実際の処理はここに
    }}
}}

";
    }

    #endregion
}

// サブカテゴリ管理クラス
public class SubCategoryManager
{
    private List<string> list = new List<string>();

    public void Add(string name) { if (!list.Contains(name)) list.Add(name); }
    public void Remove(string name) { list.Remove(name); }
    public void RemoveAt(int index) { if (index >= 0 && index < list.Count) list.RemoveAt(index); }
    public void Clear() { list.Clear(); }
    public List<string> GetAll() { return new List<string>(list); }
}
#endif