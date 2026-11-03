using UnityEditor;
using UnityEngine;

namespace ETClaimEffect
{
    [CustomEditor(typeof(ETClaimEffectMain))]
    public class ETClaimEffectMainEditor : Editor
    {
        private SerializedProperty debugOnProp;
        private SerializedProperty effectStartLocationProp;
        private SerializedProperty dataProp;

        private void OnEnable()
        {
            debugOnProp = serializedObject.FindProperty("debugOn");
            effectStartLocationProp = serializedObject.FindProperty("effectStartLocation");
            dataProp = serializedObject.FindProperty("data");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(debugOnProp);
            EditorGUILayout.PropertyField(effectStartLocationProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Claim Effect Data", EditorStyles.boldLabel);

            // Draw fields of dataProp manually
            SerializedProperty timeDelayBeforeStart = dataProp.FindPropertyRelative("timeDelayBeforeStart");
            SerializedProperty resourceDatas = dataProp.FindPropertyRelative("resourceDatas");
            SerializedProperty manualSetDestinationData = dataProp.FindPropertyRelative("manualSetDestinationData");
            SerializedProperty destinationDatas = dataProp.FindPropertyRelative("destinationDatas");
            SerializedProperty rewardParticlePrefab = dataProp.FindPropertyRelative("rewardParticlePrefab");
            SerializedProperty rewardParticleSetting = dataProp.FindPropertyRelative("rewardParticleSetting");
            SerializedProperty timeBetweenEachResourceType = dataProp.FindPropertyRelative("timeBetweenEachResourceType");
            SerializedProperty onCompletedEventDelay = dataProp.FindPropertyRelative("onCompletedEventDelay");

            EditorGUILayout.PropertyField(timeDelayBeforeStart);
            EditorGUILayout.PropertyField(resourceDatas);

            if (manualSetDestinationData != null)
            {
                EditorGUILayout.PropertyField(manualSetDestinationData);
                if (manualSetDestinationData.boolValue)
                {
                    EditorGUILayout.PropertyField(destinationDatas);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(destinationDatas);
            }

            EditorGUILayout.PropertyField(rewardParticlePrefab);
            EditorGUILayout.PropertyField(rewardParticleSetting);
            EditorGUILayout.PropertyField(timeBetweenEachResourceType);
            EditorGUILayout.PropertyField(onCompletedEventDelay);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
