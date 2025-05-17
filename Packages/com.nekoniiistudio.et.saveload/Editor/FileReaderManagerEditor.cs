using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET.FileSystem.FileReader
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(FileReaderManager))]
    public class FileReaderManagerEditor : Editor
    {
        private Vector2 _scrollPosition;
        public override void OnInspectorGUI()
        {
            // Get a reference to the FileManager target
            FileReaderManager fileManager = (FileReaderManager)target;

            // Show the default Inspector fields (for FileManager component)
            DrawDefaultInspector();

            // Display the count of loadProtocols
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Load Protocols Count", fileManager.LoadProtocols.Count.ToString());

            // Button to manually refresh NexusLinks if needed
            if (GUILayout.Button("Refresh Nexus Links"))
            {
                fileManager.RefreshNexusLinks();
            }

            // Display Nexus Links in a scrollable box
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Nexus Links", EditorStyles.boldLabel);

            if (fileManager.NexusLinks.Count > 0)
            {
                // Define scrollable area
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));

                foreach (var nexusLink in fileManager.NexusLinks)
                {
                    // Display each key-value pair (File: Link) in the nexusLinks dictionary
                    EditorGUILayout.LabelField(nexusLink.Key, nexusLink.Value);
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("No Nexus Links loaded.");
            }
        }
    }
    #endif
}

