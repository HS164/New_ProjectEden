//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UIElements;

//[UxmlElement]
//public partial class DebugModuleCard : VisualElement
//{
//    private Label m_moduleName = null;
//    private VisualElement m_cardRoot = null;

//    public DebugModuleCard()
//    {
//        var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
//            "Assets/App/Data/General/UI/System/Debug/DebugModuleCard.uxml"
//        );

//        var container = treeAsset.Instantiate();
//        hierarchy.Add(container);

//        m_moduleName = container.Q<Label>("Name");
//        m_cardRoot = container.Q<VisualElement>("Root");
//    }

//    public void SetModuleName(string moduleName, IManipulator manipulator = null)
//    {
//        m_moduleName.text = moduleName;

//        if(manipulator != null)
//        {
//            m_cardRoot.AddManipulator(manipulator);
//        }
//    }

//    public int Number { private get; set; }
//}
