using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;


namespace UtilityEditor.s_window
{
    public class ReferenceFinderWindow : EditorWindow
    {
        private const string NO_FILTER = "NO FILTER";

        [MenuItem("Utility/Reference Finder")]
        private static void ShowWindow() 
        {
            ReferenceFinderWindow win = GetWindow<ReferenceFinderWindow>();
            win.titleContent = EditorGUIUtility.IconContent("Search On Icon");
            win.titleContent.text = "Reference Finder";
            win.titleContent.tooltip = "Look for any scripts within the project-wide prefabs";
            win.Show();

            win.minSize = new Vector2(1080, 400);
        }

        public enum ExtensionType
        {
            None = 1 << 1,
            Prefab = 1 << 2,
            ScriptableObject = 1 << 3,
            All = 1 << 4,
        }

        #region PRIVATE VARIABLES
        private bool m_useString;
        private string m_assetName = "*";
        private string m_className = NO_FILTER;
        private Object m_classObject;
        private ExtensionType m_currentType = ExtensionType.None;
        private ExtensionType m_selectedType = ExtensionType.None;
        private bool m_isLoaded = false;
        private Vector2 m_scroll;
        private Object[] m_assetContainer;
        private List<GameObject> m_filteredAssets = new List<GameObject>();
        private int m_maxNameLength;
        private GUIStyle m_style;
        private GUIContent m_content;
        #endregion

        private void OnGUI()
        {
            DrawHeader();
            DrawBody();
            DrawFooter();
        }

        #region DRAW
        private void DrawHeader()
        {
            m_style = new GUIStyle(GUI.skin.label);
            m_style.alignment = TextAnchor.MiddleCenter;
            m_style.fontStyle = FontStyle.Bold;

            EditorGUILayout.BeginHorizontal("Box");

            EditorGUILayout.LabelField("Script Reference Finder", m_style, GUILayout.MaxHeight(32));

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            m_useString = EditorGUILayout.ToggleLeft(string.Empty, m_useString, GUILayout.MaxWidth(20));
            if (m_useString)
            { m_className = EditorGUILayout.TextField(m_className); }
            else
            {
                GUI.color = m_classObject ? Color.white : new Color(0.9f, 0.14f, 0.14f, 1f);
                m_classObject = EditorGUILayout.ObjectField(m_classObject, typeof(Object), true);
                GUI.color = Color.white;
            }
            m_selectedType = (ExtensionType)EditorGUILayout.EnumPopup(m_selectedType);
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawBody()
        {
            //Scroll View
            m_scroll = EditorGUILayout.BeginScrollView(m_scroll, "GroupBox");
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField("Type", GUILayout.MaxWidth(35));
            EditorGUILayout.LabelField("Name", GUILayout.MinWidth(50));
            EditorGUILayout.LabelField("Path");
            EditorGUILayout.LabelField("Actions");
            EditorGUILayout.EndHorizontal();

            if (m_filteredAssets.Count <= 0)
            {
                EditorGUILayout.HelpBox("No Matching Assets Found", MessageType.Warning);
            }
            else
            {
                foreach (Object obj in m_filteredAssets)
                {
                    SetContent();
                    if (obj != null)
                    {
                        EditorGUILayout.BeginHorizontal("Box");
                        EditorGUILayout.LabelField(m_content, GUILayout.MaxWidth(32), GUILayout.MinHeight(32));
                        EditorGUILayout.LabelField(obj.name, GUILayout.MaxWidth(m_maxNameLength * 1.5f), GUILayout.MaxHeight(32));
                        EditorGUILayout.SelectableLabel(AssetDatabase.GetAssetPath(obj), GUILayout.MaxHeight(32));

                        m_content = EditorGUIUtility.IconContent("UnityEditor.InspectorWindow@2x");
                        m_content.text = "Open";
                        if (GUILayout.Button(m_content, GUILayout.MaxHeight(32), GUILayout.MaxWidth(80)))
                        {
                            AssetDatabase.OpenAsset(obj.GetInstanceID());
                        }

                        m_content = EditorGUIUtility.IconContent("FolderOpened On Icon");
                        if (GUILayout.Button(m_content, GUILayout.MaxHeight(32), GUILayout.MaxWidth(32)))
                        {
                            string path = GetAssetFullPath(obj, m_currentType);
                            System.Diagnostics.Process.Start(path);
                        }

                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(1);
                    }
                }

            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawFooter()
        {
            DrawState();
            EditorGUILayout.BeginHorizontal();
            m_content = EditorGUIUtility.IconContent("Search On Icon");
            m_content.text = "Find Assets";
            if (GUILayout.Button(m_content, GUILayout.MinHeight(32), GUILayout.MaxHeight(32))) { Find(m_assetName); }

            m_content = EditorGUIUtility.IconContent("console.erroricon");
            m_content.text = "Clear Assets";
            if (GUILayout.Button(m_content, GUILayout.MinHeight(32), GUILayout.MaxHeight(32))) { Clear(); }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawState()
        {
            EditorGUILayout.BeginHorizontal();
            m_style = GUI.skin.label;
            m_style.alignment = TextAnchor.MiddleCenter;

            GUI.color = m_isLoaded ? Color.green : Color.red;
            EditorGUILayout.LabelField(m_isLoaded ? "Loaded" : "UnLoaded", m_style);
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region OTHER
        private void SetContent()
        {
            switch (m_currentType)
            {
                case ExtensionType.None: m_content = EditorGUIUtility.IconContent("d_DefaultAsset Icon"); break;
                case ExtensionType.Prefab: m_content = EditorGUIUtility.IconContent("d_Prefab Icon"); break;
                case ExtensionType.ScriptableObject: m_content = EditorGUIUtility.IconContent("d_ScriptableObject Icon"); break;
                default: m_content = EditorGUIUtility.IconContent("d_PrefabVariant Icon"); break;
            }
        }

        private string GetAssetFullPath(Object obj, ExtensionType type)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            string extension;
            switch (type)
            {
                case ExtensionType.Prefab: extension = "prefab"; break;
                case ExtensionType.None: extension = string.Empty; break;
                case ExtensionType.All: extension = "*"; break;
                default: extension = "*"; break;
            }
            string name = $"/{obj.name}.{extension}";

            string absolutePath = Directory.GetCurrentDirectory() + "/" + path.Replace(name, string.Empty);
            return absolutePath.Replace('/', '\\');
        }
        #endregion

        #region ACTION
        private void Find(string name)
        {
            string extension;

            m_currentType = m_selectedType;
            switch (m_currentType)
            {
                default: extension = string.Empty; break;
                case ExtensionType.Prefab: extension = "prefab"; break;
                case ExtensionType.ScriptableObject: extension = "asset"; break;
                case ExtensionType.All: extension = "*"; break;
            }

            string[] names = Directory.GetFiles("Assets", $"{name}.{extension}", SearchOption.AllDirectories);

            int max = 0;
            foreach (string n in names) { 
                max = n.Length > max ? n.Length : max;
            }
            m_maxNameLength = max;  

            m_assetContainer = new Object[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                m_assetContainer[i] = AssetDatabase.LoadAssetAtPath<Object>(names[i]);
            }

            if (m_classObject)
            {
                if(m_classObject.GetType() == typeof(MonoScript) && !m_useString)
                {
                    MonoScript script = (MonoScript)m_classObject;
                    m_className = script.GetClass().Name;
                }
            }

            Behaviour[] behaviour;
            foreach (Object obj in m_assetContainer)
            {
                GameObject go = obj as GameObject;
                if(m_className != NO_FILTER)
                {
                    behaviour = go.GetComponentsInChildren<Behaviour>();
                    var count = behaviour.Where(x => x.GetType().Name == m_className).Count();
                    if (count > 0)
                    {
                        m_filteredAssets.Add(go);
                    }
                }
                else
                {
                    m_filteredAssets.Add(go);
                }
            }

            m_isLoaded = m_assetContainer != null && m_assetContainer.Length > 0;
        }

        private void Clear()
        {
            m_isLoaded = false;
            m_assetContainer = null;
            m_filteredAssets.Clear();
            m_classObject = null;
            m_className = NO_FILTER;
        }
        #endregion
    }
}
