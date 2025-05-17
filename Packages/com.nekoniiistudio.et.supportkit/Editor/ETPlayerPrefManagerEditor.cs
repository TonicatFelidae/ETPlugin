using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET.ETPlayerPref
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ETPlayerPrefManager))]
    public class ETPlayerPrefManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ETPlayerPrefManager myScript = (ETPlayerPrefManager)target;
            GUILayout.Label("Force run initiation");
            if (GUILayout.Button("Initiation"))
            {
                myScript.Init();
            }
            GUILayout.Label("Reset all key to default value in arrays");
            if (GUILayout.Button("Reset keys to default value"))
            {
                myScript.ResetToDefaultValue();
            }
            GUILayout.Label("Waning, this will delete all list and all key in playerpref");
            if (GUILayout.Button("Delete all key"))
            {
                myScript.DeleteAllKeys();
            }
        }
    }
#endif
}
