using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

#if (UNITY_EDITOR)
namespace Tools
{
    /// <summary>
    /// This class defines the Mesh tab and it's checks as well as draws the tab specific GUI parts
    /// </summary>
    public class MeshTab : CatagoryTabBase
    {
        // The user defined criteria options are stored here
        private bool ignoreMeshRenderers;
        private bool ignoreSkinnedMeshRenderers;
        private bool checkMaterialCount;
        private bool checkVertexCount;
        private int vertexCountValue;

        public override void Draw()
        {
            base.Draw();
            // Here we draw the GUI for the filter options
            ignoreMeshRenderers = EditorGUILayout.ToggleLeft(new GUIContent("Ignore Mesh Renderers", "Do not check Mesh Renderers agains the criteria"), ignoreMeshRenderers);
            ignoreSkinnedMeshRenderers = EditorGUILayout.ToggleLeft(new GUIContent("Ignore Skinned Mesh Renderers", "Do not check Skinned Mesh Renderers agains the criteria"), ignoreSkinnedMeshRenderers);
            checkVertexCount = EditorGUILayout.BeginToggleGroup(new GUIContent("Vertex Check", "Toggle this to check the vertex count of meshes"), checkVertexCount);
            vertexCountValue = EditorGUILayout.IntSlider(new GUIContent("Vertex Count", "Mesh vertex count with more than this value will be filtered"), vertexCountValue, 0, int.MaxValue);
            EditorGUILayout.EndToggleGroup();
            checkMaterialCount = EditorGUILayout.ToggleLeft(new GUIContent("Material Check", "Check if renderers have more than one material"), checkMaterialCount);
        }

        public override void Scan()
        {
            criteriaMatches = new List<ListEntry>();

            foreach (GameObject gameObject in GetGameObjects())
            {
                // Get all the Renderers on this object and its child objects
                Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
                // check each against the criteria
                foreach (Renderer renderer in renderers)
                {
                    // Skip if we should ignore the renderer type
                    if ((ignoreMeshRenderers && renderer is MeshRenderer) || (ignoreSkinnedMeshRenderers && renderer is SkinnedMeshRenderer)) continue;

                    List<string> errors = new List<string>();

                    if (renderer is MeshRenderer) CheckIfComponentExists<MeshFilter>(renderer.gameObject, errors);


                    foreach (Object obj in CollectDependanciesRecursive(renderer.gameObject, new List<System.Type> { typeof(MeshFilter), typeof(SkinnedMeshRenderer) }, new Dictionary<int, Object>()))
                    {
                        if (obj is Mesh)
                        {
                            Mesh mesh = obj as Mesh;
                            CheckVertexCount(renderer, mesh, errors);
                        }
                    }

                    CheckMaterialCount(renderer, errors);

                    // If any of the checks were fulfilled we add a entry to the match list
                    if (errors.Count > 0) AddCriteriaMatch(renderer.gameObject, errors);
                }
            }
        }

        /// <summary>
        /// Checks if the given component type exists on the GameObject.
        /// If not add a error message.
        /// </summary>
        /// <returns>
        /// Return the component instance found
        /// </returns>
        private T CheckIfComponentExists<T>(GameObject gameObject, List<string> errors)
        {
            T component = gameObject.GetComponent<T>(); // For some reason a generic get component returns a "null" string if none was found
            if (component == null || component.ToString() == "null")
            {
                errors.Add($"No {typeof(T).Name} on gameObject.");
            }
            return component;
        }

        /// <summary>
        /// Check if the mesh passed in exsists, and adds a error message if it does not.
        /// If the mesh exists then check the vertex count
        /// </summary>
        private void CheckVertexCount (Renderer renderer, Mesh mesh, List<string> errors)
        {
            if (mesh == null) errors.Add($"No Mesh assigned for {renderer.GetType().Name}.");
            else if (checkVertexCount && mesh.vertexCount > vertexCountValue) errors.Add($"{renderer.gameObject.name} - {renderer.GetType().Name} - Vertex count of {mesh.vertexCount} exceeds {vertexCountValue} on {mesh.name}");
        }

        /// <summary>
        /// Check if there are more than one material on the given renderer and adds a error message if there are.
        /// </summary>
        private void CheckMaterialCount (Renderer renderer, List<string> errors)
        {
            if (checkMaterialCount && renderer.sharedMaterials.Length > 1) errors.Add($"{renderer.GetType().Name} Has {renderer.sharedMaterials.Length} materials");
        }
    }
}
#endif