using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)
namespace Tools {
    public class MaterialCountCriteria : CriteriaBase
    {
        public int materialCountValue = 1;

        public override System.Tuple<object, Object> GetValue(GameObject gameObject)
        {
            int materialCount = 0;
            foreach (Object obj in CollectDependanciesRecursive(gameObject, new List<System.Type> { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) }, new List<Object>()))
            {
                if (obj is Material)
                {
                    materialCount++;
                }
            }
            return new System.Tuple<object, Object>(materialCount, null);
        }

        public override bool Validate(GameObject gameObject, ref List<string> errors)
        {
            System.Tuple<object, Object> value = GetValue(gameObject);
            int materialCount = (int)value.Item1;
            bool result;
            if (materialCount > materialCountValue)
                result = false;
            else
                result = true;

            if (errors != null && !result) errors.Add($"{gameObject.name} Has {materialCount} materials assigned");

            return result;
        }
    }
}
#endif