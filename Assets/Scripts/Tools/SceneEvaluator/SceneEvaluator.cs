using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

#if (UNITY_EDITOR)
namespace Tools
{
    public class SceneEvaluator : EditorWindow
    {
        private int tabIndex = -1;
        private CatagoryTabBase activeTab;
        private static SortedList<string, System.Type> tabs = new SortedList<string, System.Type>();

        private Vector2 scroll = Vector2.zero;

        // Add menu named "SceneEvaluator" to the Window menu
        [MenuItem("24Bit Tools/SceneEvaluator %e")] //Hotkey is Ctrl-E
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            SceneEvaluator window = EditorWindow.GetWindow<SceneEvaluator>("Scene Evaluator");
            window.Show();
        }

        void OnEnable()
        {
            // When the window is enabled we check for any classes that has the ICatagoryTab interface
            FindCatagoryTabs();
        }

        static void FindCatagoryTabs()
        {
            try
            {
                System.Reflection.Assembly[] assemblys = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (System.Reflection.Assembly assembly in assemblys)
                {
                    System.Type[] types = assembly.GetTypes();
                    foreach (System.Type type in types)
                    {
                        // We check through the unity assemblies for any types that has the ICatagoryTab interface and inherits from CatagoryTabBase to add as catagory tabs in the editor window
                        if (!tabs.ContainsKey(type.Name) && type.GetInterfaces().Contains(typeof(ICatagoryTab)) && type.BaseType.Equals(typeof(CatagoryTabBase)))
                        {
                            tabs.Add(type.Name.Replace("Tab", ""), type);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                EditorWindow.GetWindow(typeof(SceneEvaluator)).Close();
            }
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Scene Evaluator", EditorStyles.boldLabel);

            // We display the tabs and create a instance of the catagory tab controller when a tab is selected
            EditorGUI.BeginChangeCheck();
            tabIndex = GUILayout.SelectionGrid(tabIndex, tabs.Keys.ToArray<string>(), tabs.Count);
            if (EditorGUI.EndChangeCheck())
            {
                activeTab = System.Activator.CreateInstance(tabs.Values[tabIndex]) as CatagoryTabBase;
            }

            if (activeTab != null)
            {
                // We draw the tab controller's GUI items first
                activeTab.Draw();

                // Then we draw the individual items that failed the validation check
                scroll = EditorGUILayout.BeginScrollView(scroll, EditorStyles.helpBox);
                for (int i = 0; i < activeTab.criteriaMatches.Count; i++)
                {
                    ListEntry entry = activeTab.criteriaMatches[i];
                    entry.foldout = EditorGUILayout.BeginFoldoutHeaderGroup(entry.foldout, $"{entry.gameObject.name} (errors: {entry.validationErrors.Count})");
                    activeTab.criteriaMatches[i] = entry;
                    if (entry.foldout)
                    {
                        // Button to select the GameObject in the scene hierarchy
                        if (GUILayout.Button("Select in scene hierarchy", EditorStyles.linkLabel))
                        {
                            EditorGUIUtility.PingObject(entry.gameObject);
                            Selection.activeObject = entry.gameObject;
                        }
                        // Display the errors that were detected
                        GUILayout.Label($"{string.Join("\n", entry.validationErrors)}");
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }
}
#endif