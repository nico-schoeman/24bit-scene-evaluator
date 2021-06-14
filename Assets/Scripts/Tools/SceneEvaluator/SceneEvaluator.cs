using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
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

        static void FindTabs()
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
                            tabs.Add(type.Name.Replace("Tab", ""), type);
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
                activeTab = System.Activator.CreateInstance(tabs.Values[tabIndex]) as CatagoryTabBase;
            }

            if (activeTab != null)
            {
                activeTab.Draw();

                EditorGUILayout.BeginScrollView(scroll);
                for (int i = 0; i < activeTab.criteriaMatches.Count; i++)
                {
                    ListEntry entry = activeTab.criteriaMatches[i];
                    entry.foldout = EditorGUILayout.BeginFoldoutHeaderGroup(entry.foldout, $"{entry.gameObject.name} (errors: {entry.validationErrors.Count})");
                    activeTab.criteriaMatches[i] = entry;
                    if (entry.foldout)
                    {
                        if (GUILayout.Button("Select in scene hierarchy", EditorStyles.linkLabel))
                        {
                            EditorGUIUtility.PingObject(entry.gameObject);
                            Selection.activeObject = entry.gameObject;
                        }
                        GUILayout.Label($"{string.Join("\n", entry.validationErrors)}");
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }

    public struct ListEntry
    {
        public GameObject gameObject;
        public List<string> validationErrors;
        public bool foldout;
    }

    public interface ICatagoryTab
    {
        void Scan();
        void Draw();
    }

    public abstract class CatagoryTabBase : ICatagoryTab
    {
        public string Name
        {
            get { return this.GetType().Name.Replace("Tab", ""); }
        }

        public List<ListEntry> criteriaMatches = new List<ListEntry>();

        public abstract void Scan();
        public virtual void Draw()
        {
            EditorGUILayout.LabelField(Name, EditorStyles.boldLabel);
            if (GUILayout.Button("Scan"))
            {
                Scan();
                Selection.objects = criteriaMatches.Select<ListEntry, GameObject>(match => { return match.gameObject; }).ToArray();
            }
        }
    }

    public class MeshTab : CatagoryTabBase
    {
        private bool ignoreMeshRenderers;
        private bool ignoreSkinnedMeshRenderers;
        private bool checkMaterialCount;
        private bool checkVertexCount;
        private int vertexCountValue;

        public override void Draw()
        {
            base.Draw();
            ignoreMeshRenderers = EditorGUILayout.ToggleLeft("Ignore Mesh Renderers", ignoreMeshRenderers);
            ignoreSkinnedMeshRenderers = EditorGUILayout.ToggleLeft("Ignore Skinned Mesh Renderers", ignoreSkinnedMeshRenderers);
            checkVertexCount = EditorGUILayout.BeginToggleGroup("Vertex count", checkVertexCount);
            vertexCountValue = EditorGUILayout.IntSlider(vertexCountValue, 0, int.MaxValue);
            EditorGUILayout.EndToggleGroup();
            checkMaterialCount = EditorGUILayout.ToggleLeft("Has more than one material", checkMaterialCount);
        }

        public override void Scan()
        {
            criteriaMatches = new List<ListEntry>();
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            GameObject[] gameObjects = scene.GetRootGameObjects();

            foreach (GameObject gameObject in gameObjects)
            {
                if (!ignoreMeshRenderers)
                {
                    MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (MeshRenderer meshRenderer in meshRenderers)
                    {
                        List<string> errors = new List<string>();
                        MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                        if (meshFilter == null) errors.Add($"No mesh filter on gameObject.");
                        if (meshFilter.sharedMesh == null) errors.Add($"MeshFilter has no Mesh assigned.");
                        else if (checkVertexCount && meshFilter.sharedMesh.vertexCount > vertexCountValue) errors.Add($"Vertex count of {meshFilter.sharedMesh.vertexCount} exceeds {vertexCountValue}");
                        if (checkMaterialCount && meshRenderer.sharedMaterials.Length > 1) errors.Add($"MeshRenderer Has {meshRenderer.sharedMaterials.Length} materials");
                        if (errors.Count > 0) AddCriteriaMatch(meshRenderer.gameObject, errors);
                    }
                }

                if (!ignoreSkinnedMeshRenderers)
                {
                    SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
                    {
                        List<string> errors = new List<string>();
                        if (skinnedMeshRenderer.sharedMesh == null) errors.Add($"SkinnedMeshRenderer has no Mesh assigned.");
                        else if (checkVertexCount && skinnedMeshRenderer.sharedMesh.vertexCount > vertexCountValue) errors.Add($"Vertex count of {skinnedMeshRenderer.sharedMesh.vertexCount} exceeds {vertexCountValue}");
                        if (checkMaterialCount && skinnedMeshRenderer.sharedMaterials.Length > 1) errors.Add($"SkinnedMeshRenderer Has {skinnedMeshRenderer.sharedMaterials.Length} materials");
                        if (errors.Count > 0) AddCriteriaMatch(skinnedMeshRenderer.gameObject, errors);
                    }
                }
            }
        }

        private void AddCriteriaMatch(GameObject gameObject, List<string> errors)
        {
            if (criteriaMatches.Any(match => match.gameObject == gameObject))
            {
                ListEntry entry = criteriaMatches.Single(match => match.gameObject == gameObject);
                entry.validationErrors = entry.validationErrors.Union(errors).ToList();
            }
            else
            {
                criteriaMatches.Add(new ListEntry() { gameObject = gameObject, validationErrors = errors });
            }
        }
    }
}
#endif