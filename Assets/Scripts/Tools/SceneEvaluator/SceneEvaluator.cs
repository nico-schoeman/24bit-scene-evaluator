using System;
using System.Collections;
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
        private static SortedList<string, System.Type> tabs = new SortedList<string, System.Type>();

        // Add menu named "SceneEvaluator" to the Window menu
        [MenuItem("24bit Tools/SceneEvaluator %e")] //Hotkey is Ctrl-E
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            SceneEvaluator window = (SceneEvaluator)EditorWindow.GetWindow(typeof(SceneEvaluator));
            window.Show();
        }

        void OnEnable()
        {
            FindTabs();
        }

        static void FindTabs ()
        {
            try
            {
                System.Reflection.Assembly[] assemblys = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (System.Reflection.Assembly assembly in assemblys)
                {
                    System.Type[] types = assembly.GetTypes();
                    foreach (System.Type type in types)
                    {
                        if (!tabs.ContainsKey(type.Name) && type.GetInterfaces().Contains(typeof(ICatagoryTab)) && type.BaseType.Equals(typeof(CatagoryTabBase)))
                        {
                            tabs.Add(type.Name, type);
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

            EditorGUI.BeginChangeCheck();
            tabIndex = GUILayout.SelectionGrid(tabIndex, tabs.Keys.ToArray<string>(), tabs.Count);
            if (EditorGUI.EndChangeCheck())
            {
                CatagoryTabBase tab = System.Activator.CreateInstance(tabs.Values[tabIndex]) as CatagoryTabBase;
                tab.Draw();
            }
        }
    }

    public interface ICatagoryTab
    {
        void Scan();
        void Draw();
    }

    public abstract class CatagoryTabBase: ICatagoryTab
    {
        string name;
        public abstract void Scan();
        public void Draw()
        {
            Debug.Log("test");
        }
    }

    public class MeshTab: CatagoryTabBase
    {
        string name = "Mesh";
        public override void Scan ()
        {

        }
    }
}
#endif