using UnityEngine;
using UnityEditor;
using System.IO;

public class ETScriptCreator : EditorWindow
{
    private string _UIScreenScriptName = "UIScreenScriptName";

    [MenuItem("ETools/ETScriptCreator")]
    public static void ShowWindow()
    {
        GetWindow<ETScriptCreator>("ETScriptCreator");
    }

    private void OnGUI()
    {
        GUILayout.Label("UI Script Generator", EditorStyles.boldLabel);
        _UIScreenScriptName = EditorGUILayout.TextField("ScriptName", _UIScreenScriptName);

        if (GUILayout.Button("Create UIScreenScript"))
        {
            CreateUIScript(_UIScreenScriptName);
        }
        if (GUILayout.Button("Create PopupUIScreenScript"))
        {
            CreatePopupUIScript(_UIScreenScriptName);
        }
    }

    private void CreateUIScript(string className)
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save UI Script",
            className + ".cs",
            "cs",
            "Enter a file name to save the script"
        );

        if (string.IsNullOrEmpty(path)) return;

        string content =
$@"using ET.UIKit.ZenjectUIScreen;
namespace Game.UI
{{
    public class {className} : UIScreen
    {{
    }}
}}";
        File.WriteAllText(path, content);
        AssetDatabase.Refresh();
    }
    private void CreatePopupUIScript(string className)
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save UI Script",
            className + ".cs",
            "cs",
            "Enter a file name to save the script"
        );

        if (string.IsNullOrEmpty(path)) return;

        string content =
$@"using ET.UIKit.ZenjectUIScreen;
namespace Game.UI
{{
    public class {className} : PopupUIScreen
    {{
    }}
}}";
        File.WriteAllText(path, content);
        AssetDatabase.Refresh();
    }
}