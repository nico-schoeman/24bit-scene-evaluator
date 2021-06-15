using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

#if (UNITY_EDITOR)
namespace Tools
{
    /// <summary>
    /// This class defines the Material tab and it's checks as well as draws the tab specific GUI parts
    /// </summary>
    public class MaterialTab : CatagoryTabBase
    {
        // The user defined criteria options are stored here
        private bool checkTexture;
        private int textureDimension;
        private bool checkShader;
        private int shaderIndex;
        private string shaderName;

        private string[] shaderNames;

        public override void Draw()
        {
            base.Draw();

            // Here we draw the GUI for the filter options
            checkTexture = EditorGUILayout.BeginToggleGroup(new GUIContent("Vertex Check", "Toggle this to check the vertex count of meshes"), checkTexture);
            textureDimension = EditorGUILayout.IntSlider(new GUIContent("Vertex Count", "Mesh vertex count with more than this value will be filtered"), textureDimension, 0, 8192);
            EditorGUILayout.EndToggleGroup();

            EditorGUI.BeginChangeCheck();
            checkShader = EditorGUILayout.BeginToggleGroup(new GUIContent("Vertex Check", "Toggle this to check the vertex count of meshes"), checkShader);
            if (EditorGUI.EndChangeCheck())
            {
                ShaderInfo[] shaderInfos = ShaderUtil.GetAllShaderInfo();
                shaderNames = shaderInfos.Select(shader => { return shader.name; }).ToArray();
            }

            shaderIndex = EditorGUILayout.Popup(new GUIContent("Shader"), shaderIndex, shaderNames != null ? shaderNames : new string[] { });
            if (shaderNames != null) shaderName = shaderNames[shaderIndex];
            EditorGUILayout.EndToggleGroup();
        }

        public override void Scan()
        {
            criteriaMatches = new List<ListEntry>();

            foreach (GameObject gameObject in GetGameObjects())
            {
                Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                {
                    List<string> errors = new List<string>();

                    foreach (Object obj in CollectDependanciesRecursive(renderer.gameObject, new List<System.Type> { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) }, new Dictionary<int, Object>()))
                    {
                        if (checkTexture && obj is Texture)
                        {
                            Texture texture = obj as Texture;
                            if (texture.width > textureDimension || texture.height > textureDimension) errors.Add($"{texture.name} {texture.width}x{texture.height} Texture dimentions exceeds {textureDimension}");
                        }

                        if (checkShader && obj is Shader)
                        {
                            Shader shader = obj as Shader;
                            if (shader.name == shaderName) errors.Add($"Found {shaderName} shader on {renderer.gameObject.name}");
                        }
                    }

                    if (errors.Count > 0) AddCriteriaMatch(renderer.gameObject, errors);
                }
            }
        }
    }
}
#endif