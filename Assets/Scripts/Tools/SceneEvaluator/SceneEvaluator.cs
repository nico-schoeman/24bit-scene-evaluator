using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

#if (UNITY_EDITOR)
namespace Tools
{
    public class SceneEvaluator : EditorWindow
    {
        private int tabIndex = -1;
        private CategoryTabBase activeTab;
        private static SortedList<string, System.Type> tabs = new SortedList<string, System.Type>();

        private Vector2 scroll = Vector2.zero;
        private static SceneEvaluator window;

        // Add menu named "SceneEvaluator" to the Window menu
        [MenuItem("24Bit Tools/SceneEvaluator %e")] //Hotkey is Ctrl-E
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = EditorWindow.GetWindow<SceneEvaluator>("Scene Evaluator");
            window.Show();
        }

        void OnEnable()
        {
            // When the window is enabled we check for any classes that has the IcategoryTab interface
            FindControllers<ICategoryTab, CategoryTabBase>(ref tabs);
        }

        static void FindControllers<I, T>(ref SortedList<string, Type> collection)
        {
            try
            {
                System.Reflection.Assembly[] assemblys = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (System.Reflection.Assembly assembly in assemblys)
                {
                    System.Type[] types = assembly.GetTypes();
                    foreach (System.Type type in types)
                    {
                        // We check through the unity assemblies for any types that has the IcategoryTab interface and inherits from categoryTabBase to add as category tabs in the editor window
                        if (!collection.ContainsKey(type.Name) && type.GetInterfaces().Contains(typeof(I)) && type.BaseType.Equals(typeof(T)))
                        {
                            collection.Add(type.Name.Replace("Tab", ""), type);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Scene Evaluator", EditorStyles.boldLabel);

            // We display the tabs and create a instance of the category tab controller when a tab is selected
            EditorGUI.BeginChangeCheck();
            tabIndex = GUILayout.SelectionGrid(tabIndex, tabs.Keys.ToArray<string>(), tabs.Count);
            if (EditorGUI.EndChangeCheck())
            {
                activeTab = System.Activator.CreateInstance(tabs.Values[tabIndex]) as CategoryTabBase;
            }

            // Button to export criteria values for select game objects
            if (GUILayout.Button("Export Selected"))
            {
                ExportToCSV();
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

        /// <summary>
        /// Export the current scene selection to CSV.
        /// Each of the criteria are run for each of the select items
        /// </summary>
        public void ExportToCSV()
        {
            // Let the user choose a name and directory
            string path = EditorUtility.SaveFilePanel("Export Scene Evaluation", "", $"SceneEvaluation-{EditorSceneManager.GetActiveScene().name}-{System.DateTime.Now.ToShortDateString().Replace("/","-")}.csv", "csv");

            if (string.IsNullOrWhiteSpace(path)) return;

            // Dynamically find all the criteria classes
            SortedList<string, Type> criteriaTypes = new SortedList<string, Type>();
            FindControllers<ICriteria, CriteriaBase>(ref criteriaTypes);
            List<CriteriaBase> criterias = new List<CriteriaBase>();

            // Create a instance of each we can use
            foreach (var criteriaType in criteriaTypes)
            {
                criterias.Add(Activator.CreateInstance(criteriaType.Value) as CriteriaBase);
            }

            // Setup the writer
            System.IO.StreamWriter sw = new System.IO.StreamWriter(path);
            // Write the header row
            sw.WriteLine($"GameObject,{string.Join(",", criterias.Select(item => { return item.GetType().Name.Replace("Criteria", ""); }))}");

            foreach (GameObject item in Selection.objects)
            {
                List<string> data = new List<string> { item.name };

                // Run each criteria for each of the selected objects
                foreach (var criteria in criterias)
                {
                    string value = criteria.GetValue(item).Item1.ToString();
                    data.Add(value == "-1" || string.IsNullOrWhiteSpace(value) ? "NAN" : value);
                }

                // comma sepperate the data and write the line
                string line = string.Join(",", data);
                sw.WriteLine(line);
            }

            sw.Close();
        }
    }
}
#endif