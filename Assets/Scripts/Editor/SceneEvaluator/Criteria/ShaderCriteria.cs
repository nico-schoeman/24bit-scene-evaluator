#if (UNITY_EDITOR)
namespace Tools
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ShaderCriteria : CriteriaBase
    {
        public int ShaderIndex { get; set; }
        public string ShaderName { get; set; }

        public override System.Tuple<object, Object> GetValue(GameObject gameObject)
        {
            foreach (Object obj in CollectDependanciesRecursive(gameObject, new List<System.Type> { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) }, new List<Object>()))
            {
                if (obj is Shader)
                {
                    Shader shader = obj as Shader;
                    // We get the shader name
                    return new System.Tuple<object, Object>(shader.name, shader);
                }
            }
            return new System.Tuple<object, Object>("", null);
        }

        public override bool Validate(GameObject gameObject, ref List<string> errors)
        {
            // Extract the values
            System.Tuple<object, Object> value = GetValue(gameObject);
            Shader shader = value.Item2 as Shader;

            // Validate the values
            bool result = shader?.name != ShaderName;

            // Add a validation error if nessisary
            if (errors != null && !result) errors.Add($"Found {ShaderName} shader on {gameObject.name}");

            return result;
        }
    }
}
#endif