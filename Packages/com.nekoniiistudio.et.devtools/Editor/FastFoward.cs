using UnityEngine;
using UnityEditor;
using System.Globalization;

namespace ET
{
    public class TimeScaleWindow : EditorWindow
    {
        private readonly string[] labels = { "x0.25", "x0.5", "x0.75", "x1", "x2", "x5", "x10", "x100" };
        private readonly float[] values = { 0.25f, 0.5f, 0.75f, 1f, 2f, 5f, 10f, 100f };

        private string timeScaleText = "1";
        private float lastKnownScale = 1f;

        [MenuItem("ETools/Time Scale")]
        public static void ShowWindow()
        {
            GetWindow<TimeScaleWindow>("Time Scale");
        }

        private void OnEnable()
        {
            lastKnownScale = Time.timeScale;
            timeScaleText = lastKnownScale.ToString(CultureInfo.InvariantCulture);
            EditorApplication.update += EditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            // If time scale changed externally, update the text field and repaint
            if (!Mathf.Approximately(Time.timeScale, lastKnownScale))
            {
                lastKnownScale = Time.timeScale;
                timeScaleText = lastKnownScale.ToString(CultureInfo.InvariantCulture);
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Quick Time Scale", EditorStyles.boldLabel);

            // Text field showing current time scale (editable)
            EditorGUILayout.BeginHorizontal();
            timeScaleText = EditorGUILayout.TextField("Current Time Scale", timeScaleText);
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                ApplyTextField();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6);

            // 4 columns x 2 rows grid of buttons
            int cols = 4;
            int rows = 2;
            int idx = 0;
            for (int r = 0; r < rows; r++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int c = 0; c < cols; c++)
                {
                    if (idx >= labels.Length) break;
                    if (GUILayout.Button(labels[idx], GUILayout.Height(30)))
                    {
                        SetTimeScale(values[idx]);
                    }

                    idx++;
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();

            // Small footer showing exact Time.timeScale
            EditorGUILayout.LabelField("Exact Time.timeScale:", Time.timeScale.ToString(CultureInfo.InvariantCulture));
        }

        private void ApplyTextField()
        {
            if (float.TryParse(timeScaleText, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            {
                SetTimeScale(v);
            }
            else
            {
                // invalid input: restore text to current time scale
                timeScaleText = Time.timeScale.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void SetTimeScale(float value)
        {
            Time.timeScale = value;
            lastKnownScale = value;
            timeScaleText = value.ToString(CultureInfo.InvariantCulture);
            Repaint();
        }
    }
}