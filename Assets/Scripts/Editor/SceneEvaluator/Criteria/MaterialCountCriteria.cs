#if (UNITY_EDITOR)
namespace Tools
{
    using System.Collections.Generic;
    using UnityEngine;

    public class MaterialCountCriteria : CriteriaBase
    {
        public int MaterialCountValue { get; set; } = 1;

        public override System.Tuple<object, Object> GetValue(GameObject gameObject)
        {
            int materialCount = 0;
            foreach (Object obj in CollectDependanciesRecursive(gameObject, new List<System.Type> { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) }, new List<Object>()))
            {
                if (obj is Material)
                {
                    // Incriment for each material we find in the whitlisted component types
                    materialCount++;
                }
            }
            // Return a Tuple, in this case we only care about the incremented material count
            return new System.Tuple<object, Object>(materialCount, null);
        }

        public override bool Validate(GameObject gameObject, ref List<string> errors)
        {
            // Extract the values
            System.Tuple<object, Object> value = GetValue(gameObject);
            int materialCount = (int)value.Item1;

            // Validate the values
            bool result = materialCount <= MaterialCountValue;

            // Add a validation error if nessisary
            if (errors != null && !result) errors.Add($"{gameObject.name} Has {materialCount} materials assigned");

            return result;
        }
    }
}
#endif