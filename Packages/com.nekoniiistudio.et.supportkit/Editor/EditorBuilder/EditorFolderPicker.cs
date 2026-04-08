
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
#if UNITY_EDITOR
    /// <summary>
    /// Reusable folder picker backed by EditorPrefs — survives domain reloads.
    /// Usage:
    ///   var picker = new EditorFolderPicker("MyKey", "Assets/Resources/Tiles");
    ///   picker.DrawLayout("Label text");
    ///   string path = picker.SelectedPath;
    /// </summary>
    public class EditorFolderPicker
    {
        private readonly string _prefKey;
        private readonly string _defaultPath;
        private readonly string _panelTitle;

        public string SelectedPath
        {
            get => EditorPrefs.GetString(_prefKey, "");
            private set => EditorPrefs.SetString(_prefKey, value);
        }

        public EditorFolderPicker(string prefKey, string defaultPath, string panelTitle = "Select Folder")
        {
            _prefKey = prefKey;
            _defaultPath = defaultPath;
            _panelTitle = panelTitle;
        }

        /// <summary>Converts the absolute OS path to a project-relative Assets/... path.</summary>
        public string AssetFolderPath
        {
            get
            {
                string path = SelectedPath;
                if (string.IsNullOrEmpty(path)) return "";
                string dataPath = Application.dataPath;
                if (path.StartsWith(dataPath))
                    return "Assets" + path.Substring(dataPath.Length);
                return path;
            }
        }

        /// <summary>True when AssetFolderPath points to a real folder inside the project.</summary>
        public bool IsValidAssetFolder => !string.IsNullOrEmpty(AssetFolderPath) && AssetDatabase.IsValidFolder(AssetFolderPath);

        /// <summary>Loads all assets of type T found inside the selected folder.</summary>
        public T[] LoadAll<T>() where T : UnityEngine.Object
        {
            string folder = AssetFolderPath;
            if (!AssetDatabase.IsValidFolder(folder)) return System.Array.Empty<T>();

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            var results = new System.Collections.Generic.List<T>(guids.Length);
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) results.Add(asset);
            }
            return results.ToArray();
        }

        /// <summary>Draws the label + button and returns true when a new folder was chosen.</summary>
        public bool DrawLayout(string label)
        {
            string current = SelectedPath;
            EditorGUILayout.LabelField(label, string.IsNullOrEmpty(current) ? "(none)" : current, EditorStyles.helpBox);

            if (GUILayout.Button($"Select {label}"))
            {
                string openIn = string.IsNullOrEmpty(current) ? _defaultPath : current;
                string chosen = EditorUtility.OpenFolderPanel(_panelTitle, openIn, "");
                if (!string.IsNullOrEmpty(chosen))
                {
                    SelectedPath = chosen;
                    return true;
                }
            }

            return false;
        }
    }
#endif
}