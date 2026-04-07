using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ET.SupportKit
{
    public class AssetAtlas : MonoBehaviour
    {
        public List<string> assetPaths = new List<string>();

    }
#if UNITY_EDITOR
    [CustomEditor(typeof(AssetAtlas))]
    public class AssetAtlasEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            AssetAtlas atlas = (AssetAtlas)target;
            EditorGUILayout.Space(4);

            // ── Asset path list ──
            EditorGUILayout.LabelField("Asset Paths", EditorStyles.boldLabel);
            for (int i = 0; i < atlas.assetPaths.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                atlas.assetPaths[i] = EditorGUILayout.TextField(atlas.assetPaths[i]);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    string defaultPath = string.IsNullOrEmpty(atlas.assetPaths[i]) ? Application.dataPath + "/Resources/Tiles" : atlas.assetPaths[i];
                    string chosen = EditorUtility.OpenFolderPanel("Select Tile Directory", defaultPath, "");
                    if (!string.IsNullOrEmpty(chosen))
                    {
                        atlas.assetPaths[i] = chosen;
                        EditorUtility.SetDirty(atlas);
                    }
                }
                if (GUILayout.Button("-", GUILayout.Width(24)))
                {
                    atlas.assetPaths.RemoveAt(i);
                    EditorUtility.SetDirty(atlas);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Folder"))
            {
                string defaultPath = Application.dataPath + "/Resources/Tiles";
                string chosen = EditorUtility.OpenFolderPanel("Select Tile Directory", defaultPath, "");
                if (!string.IsNullOrEmpty(chosen))
                {
                    atlas.assetPaths.Add(chosen);
                    EditorUtility.SetDirty(atlas);
                }
            }

            EditorGUILayout.Space(6);
        }
    }
#endif
}