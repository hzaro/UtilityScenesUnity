using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using System.IO;
using Codice.Utils;

namespace CoreEditor
{
    public class ThumbnailWindow : EditorWindow
    {
        private static ThumbnailWindow s_window;

        private Object m_target;
        private string m_path;

        [MenuItem("Utility/Thumbnail")]
        static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            s_window = (ThumbnailWindow)EditorWindow.GetWindow(typeof(ThumbnailWindow));
            s_window.Show();

            GUIContent content = EditorGUIUtility.IconContent("Camera Gizmo");
            content.text = "Thumbnail Generator";
            content.tooltip = "Generate a object thumbnail to use ingame [This class Only Work in The Unity Editor]";

            s_window.titleContent = content;
            s_window.m_path = Application.dataPath;

            s_window.minSize = new Vector2(350,400);
        }

        float m_xProgress, m_yProgress;
        void OnGUI()
        {

            EditorGUILayout.LabelField("Thumbnail Settings", EditorStyles.boldLabel);

            EditorGUILayout.TextField(m_path);
            m_target = EditorGUILayout.ObjectField("Target", m_target, typeof(Object), true);

            AssetPreview.SetPreviewTextureCacheSize(256);
            Texture2D image = AssetPreview.GetAssetPreview(m_target);
            EditorGUILayout.Space();
            if (image)
            {
                float xpivot = image.width / 2;
                float ypivot = image.height / 2;

                float x = (s_window.position.width * 0.5f) - xpivot;
                float y = (s_window.position.height * 0.5f) - ypivot;

                y = Mathf.Clamp(y, 120, s_window.position.height);
                Rect rect = new Rect(x, y, image.width, image.height);

                EditorGUI.DrawPreviewTexture(rect, image, null, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUILayout.HelpBox("Select an Object to generate a image preview", MessageType.Warning);
            }

            

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Generate Texture"))
            {
                if (image)
                {
                    byte[] bytes = image.EncodeToPNG();
                    File.WriteAllBytes(m_path + "/" + m_target.name + ".png", bytes);
                }
            }

            if(GUILayout.Button("Path"))
            {
                m_path = EditorUtility.OpenFolderPanel("Save Path", Application.dataPath, "None");
            }

            if(GUILayout.Button("Show In Explorer"))
            {
                EditorUtility.RevealInFinder(m_path);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
