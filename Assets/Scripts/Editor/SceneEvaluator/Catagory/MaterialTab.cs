#if (UNITY_EDITOR)
namespace Tools
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// This class defines the Material tab and it's checks as well as draws the tab specific GUI parts
    /// </summary>
    public class MaterialTab : CategoryTabBase
    {
        // The criteria options are defined here
        private readonly TextureDimensionCriteria _textureDimensionCriteria = new TextureDimensionCriteria();
        private readonly ShaderCriteria _shaderCriteria = new ShaderCriteria();

        private bool _checkTexture;
        private bool _checkShader;

        private string[] _shaderNames;

        public override void Draw()
        {
            base.Draw();

            // Here we draw the GUI for the filter options
            _checkTexture = EditorGUILayout.BeginToggleGroup(new GUIContent("Texture Check", "Toggle this to check the texture dimentions of materials"), _checkTexture);
            _textureDimensionCriteria.TextureDimension = EditorGUILayout.IntSlider(new GUIContent("Texture Dimention", "If either of the texture dimentions exceed this amout it will be filtered"), _textureDimensionCriteria.TextureDimension, 0, 8192);
            EditorGUILayout.EndToggleGroup();

            // Check if shader toggle has changed, if it has we get all the shader names
            EditorGUI.BeginChangeCheck();
            _checkShader = EditorGUILayout.BeginToggleGroup(new GUIContent("Shader Check", "Toggle this to check filter game objects based on the shader used in their materials"), _checkShader);
            if (EditorGUI.EndChangeCheck())
            {
                ShaderInfo[] shaderInfos = ShaderUtil.GetAllShaderInfo();
                _shaderNames = shaderInfos.Select(shader => shader.name).ToArray();
            }

            // Shader selection
            _shaderCriteria.ShaderIndex = EditorGUILayout.Popup(new GUIContent("Shader Select", "Select the shader to filter for"), _shaderCriteria.ShaderIndex, _shaderNames ?? (new string[] { }));
            if (_shaderNames != null) _shaderCriteria.ShaderName = _shaderNames[_shaderCriteria.ShaderIndex];
            EditorGUILayout.EndToggleGroup();
        }

        public override void Scan()
        {
            CriteriaMatches = new List<ListEntry>();

            foreach (GameObject gameObject in GetGameObjects())
            {
                Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                {
                    List<string> errors = new List<string>();

                    // Run the appropriate criteria validations depending on filter options selected
                    if (_checkTexture) _textureDimensionCriteria.Validate(renderer.gameObject, ref errors);
                    if (_checkShader) _shaderCriteria.Validate(renderer.gameObject, ref errors);

                    if (errors.Count > 0) AddCriteriaMatch(renderer.gameObject, errors);
                }
            }
        }
    }
}
#endif