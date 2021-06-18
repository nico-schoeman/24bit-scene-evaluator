#if (UNITY_EDITOR)
namespace Tools
{
    using System.Collections.Generic;
    using UnityEngine;

    public class VertexCountCriteria : CriteriaBase
    {
        public int VertexCountValue { get; set; }

        public override System.Tuple<object, Object> GetValue(GameObject gameObject)
        {
            foreach (Object obj in CollectDependanciesRecursive(gameObject, new List<System.Type> { typeof(MeshFilter), typeof(SkinnedMeshRenderer) }, new List<Object>()))
            {
                if (obj is Mesh)
                {
                    Mesh mesh = obj as Mesh;
                    // Get the mesh's vertex count
                    return new System.Tuple<object, Object>(mesh.vertexCount, mesh);
                }
            }
            return new System.Tuple<object, Object>(-1, null);
        }

        public override bool Validate(GameObject gameObject, ref List<string> errors)
        {
            // Extract the values
            System.Tuple<object, Object> value = GetValue(gameObject);
            Mesh mesh = value.Item2 as Mesh;
            int vertexCount = (int)value.Item1;

            // Validate the values
            bool result = vertexCount <= VertexCountValue;

            //add a validation error if nessisary
            if (errors != null && !result) errors.Add($"{gameObject.name} - {mesh.name} - Vertex count of {vertexCount} exceeds {VertexCountValue}");

            return result;
        }
    }
}
#endif