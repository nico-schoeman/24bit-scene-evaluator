﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)
namespace Tools {
    public class ShaderCriteria : CriteriaBase
    {
        public int shaderIndex;
        public string shaderName;

        public override System.Tuple<object, Object> GetValue(GameObject gameObject)
        {
            foreach (Object obj in CollectDependanciesRecursive(gameObject, new List<System.Type> { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) }, new List<Object>()))
            {
                if (obj is Shader)
                {
                    Shader shader = obj as Shader;
                    // We get the shader name
                    return new System.Tuple<object, Object> (shader.name, shader);
                }
            }
            return new System.Tuple<object, Object>("", null);
        }

        public override bool Validate(GameObject gameObject, ref List<string> errors)
        {
            // Extract the values
            System.Tuple<object, Object> value = GetValue(gameObject);
            Shader shader = value.Item2 as Shader;

            bool result;
            // Validate the values
            if (shader.name == shaderName) 
                result = false;
            else
                result = true;

            //add a validation error if nessisary
            if (errors != null && !result) errors.Add($"Found {shaderName} shader on {gameObject.name}");

            return result;
        }
    }
}
#endif