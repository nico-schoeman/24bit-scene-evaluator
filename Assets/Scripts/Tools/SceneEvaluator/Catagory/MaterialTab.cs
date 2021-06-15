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
        private TextureDimensionCriteria textureDimensionCriteria = new TextureDimensionCriteria();
        private ShaderCriteria shaderCriteria = new ShaderCriteria();

        private bool checkTexture;
        private bool checkShader;

        private string[] shaderNames;

        public override void Draw()
        {
            base.Draw();

            // Here we draw the GUI for the filter options
            checkTexture = EditorGUILayout.BeginToggleGroup(new GUIContent("Texture Check", "Toggle this to check the texture dimentions of materials"), checkTexture);
            textureDimensionCriteria.textureDimension = EditorGUILayout.IntSlider(new GUIContent("Texture Dimention", "If either of the texture dimentions exceed this amout it will be filtered"), textureDimensionCriteria.textureDimension, 0, 8192);
            EditorGUILayout.EndToggleGroup();

            EditorGUI.BeginChangeCheck();
            checkShader = EditorGUILayout.BeginToggleGroup(new GUIContent("Shader Check", "Toggle this to check filter game objects based on the shader used in their materials"), checkShader);
            if (EditorGUI.EndChangeCheck())
            {
                ShaderInfo[] shaderInfos = ShaderUtil.GetAllShaderInfo();
                shaderNames = shaderInfos.Select(shader => { return shader.name; }).ToArray();
            }

            shaderCriteria.shaderIndex = EditorGUILayout.Popup(new GUIContent("Shader"), shaderCriteria.shaderIndex, shaderNames != null ? shaderNames : new string[] { });
            if (shaderNames != null) shaderCriteria.shaderName = shaderNames[shaderCriteria.shaderIndex];
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

                    if (checkTexture) textureDimensionCriteria.Validate(renderer.gameObject, ref errors);
                    if (checkShader) shaderCriteria.Validate(renderer.gameObject, ref errors);

                    if (errors.Count > 0) AddCriteriaMatch(renderer.gameObject, errors);
                }
            }
        }
    }
}
#endif