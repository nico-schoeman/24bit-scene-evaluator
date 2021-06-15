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
    public class MaterialTab : CategoryTabBase
    {
        // The criteria options are defined here
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

            // Check if shader toggle has changed, if it has we get all the shader names
            EditorGUI.BeginChangeCheck();
            checkShader = EditorGUILayout.BeginToggleGroup(new GUIContent("Shader Check", "Toggle this to check filter game objects based on the shader used in their materials"), checkShader);
            if (EditorGUI.EndChangeCheck())
            {
                ShaderInfo[] shaderInfos = ShaderUtil.GetAllShaderInfo();
                shaderNames = shaderInfos.Select(shader => { return shader.name; }).ToArray();
            }

            // Shader selection
            shaderCriteria.shaderIndex = EditorGUILayout.Popup(new GUIContent("Shader Select", "Select the shader to filter for"), shaderCriteria.shaderIndex, shaderNames != null ? shaderNames : new string[] { });
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

                    // run the appropriate criteria validations depending on filter options selected
                    if (checkTexture) textureDimensionCriteria.Validate(renderer.gameObject, ref errors);
                    if (checkShader) shaderCriteria.Validate(renderer.gameObject, ref errors);

                    if (errors.Count > 0) AddCriteriaMatch(renderer.gameObject, errors);
                }
            }
        }
    }
}
#endif