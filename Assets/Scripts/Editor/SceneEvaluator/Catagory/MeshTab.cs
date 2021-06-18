#if (UNITY_EDITOR)
namespace Tools
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// This class defines the Mesh tab and it's checks as well as draws the tab specific GUI parts
    /// </summary>
    public class MeshTab : CategoryTabBase
    {
        // The criteria options are defined here
        private readonly VertexCountCriteria _vertexCountCriteria = new VertexCountCriteria();
        private readonly MaterialCountCriteria _materialCountCriteria = new MaterialCountCriteria();

        private bool _ignoreMeshRenderers;
        private bool _ignoreSkinnedMeshRenderers;
        private bool _checkMaterialCount;
        private bool _checkVertexCount;

        public override void Draw()
        {
            base.Draw();
            // Here we draw the GUI for the filter options
            _ignoreMeshRenderers = EditorGUILayout.ToggleLeft(new GUIContent("Ignore Mesh Renderers", "Do not check Mesh Renderers agains the criteria"), _ignoreMeshRenderers);
            _ignoreSkinnedMeshRenderers = EditorGUILayout.ToggleLeft(new GUIContent("Ignore Skinned Mesh Renderers", "Do not check Skinned Mesh Renderers agains the criteria"), _ignoreSkinnedMeshRenderers);

            _checkVertexCount = EditorGUILayout.BeginToggleGroup(new GUIContent("Vertex Check", "Toggle this to check the vertex count of meshes"), _checkVertexCount);
            _vertexCountCriteria.VertexCountValue = EditorGUILayout.IntSlider(new GUIContent("Vertex Count", "Mesh vertex count with more than this value will be filtered"), _vertexCountCriteria.VertexCountValue, 0, int.MaxValue);
            EditorGUILayout.EndToggleGroup();

            _checkMaterialCount = EditorGUILayout.ToggleLeft(new GUIContent("Material Check", "Check if renderers have more than one material"), _checkMaterialCount);
        }

        public override void Scan()
        {
            CriteriaMatches = new List<ListEntry>();

            foreach (GameObject gameObject in GetGameObjects())
            {
                // Get all the Renderers on this object and its child objects
                Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
                // Check each against the criteria
                foreach (Renderer renderer in renderers)
                {
                    // Skip if we should ignore the renderer type
                    if ((_ignoreMeshRenderers && renderer is MeshRenderer) || (_ignoreSkinnedMeshRenderers && renderer is SkinnedMeshRenderer)) continue;

                    List<string> errors = new List<string>();

                    // Additional validation to check if there is a MeshFilter attached
                    if (renderer is MeshRenderer) CheckIfComponentExists<MeshFilter>(renderer.gameObject, errors);

                    // Run the appropriate criteria validations depending on filter options selected
                    if (_checkVertexCount) _vertexCountCriteria.Validate(renderer.gameObject, ref errors);
                    if (_checkMaterialCount) _materialCountCriteria.Validate(renderer.gameObject, ref errors);

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
        private T CheckIfComponentExists<T>(GameObject gameObject, List<string> errors) where T : Component
        {
            // For some reason a generic get component returns a "null" string if none was found
            T component = gameObject.GetComponent<T>();
            if (component == null || component.ToString() == "null")
            {
                errors.Add($"No {typeof(T).Name} on gameObject.");
            }
            return component;
        }
    }
}
#endif