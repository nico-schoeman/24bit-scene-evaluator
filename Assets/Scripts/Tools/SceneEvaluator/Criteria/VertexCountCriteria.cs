using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)
namespace Tools {
    public class VertexCountCriteria : CriteriaBase
    {
        public int vertexCountValue;

        public override System.Tuple<object, Object> GetValue(GameObject gameObject)
        {
            foreach (Object obj in CollectDependanciesRecursive(gameObject, new List<System.Type> { typeof(MeshFilter), typeof(SkinnedMeshRenderer) }, new List<Object>()))
            {
                if (obj is Mesh)
                {
                    Mesh mesh = obj as Mesh;
                    return new System.Tuple<object, Object>(mesh.vertexCount, mesh);
                }
            }
            return new System.Tuple<object, Object>(-1, null);
        }

        public override bool Validate(GameObject gameObject, ref List<string> errors)
        {
            System.Tuple<object, Object> value = GetValue(gameObject);
            Mesh mesh = value.Item2 as Mesh;
            int vertexCount = (int)value.Item1;
            bool result;
            if (vertexCount > vertexCountValue)
                result = false;
            else
                result = true;

            if (errors != null && !result) errors.Add($"{gameObject.name} - {mesh.name} - Vertex count of {vertexCount} exceeds {vertexCountValue}");

            return result;
        }
    }
}
#endif