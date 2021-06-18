#if (UNITY_EDITOR)
namespace Tools
{
    using System.Collections.Generic;
    using UnityEngine;

    public class TextureDimensionCriteria : CriteriaBase
    {
        public int TextureDimension { get; set; }

        public override System.Tuple<object, Object> GetValue(GameObject gameObject)
        {
            foreach (Object obj in CollectDependanciesRecursive(gameObject, new List<System.Type> { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) }, new List<Object>()))
            {
                if (obj is Texture)
                {
                    Texture texture = obj as Texture;
                    // We check whether the width or hight is the highest, and use the higher value
                    return new System.Tuple<object, Object>(Mathf.Max(texture.width, texture.height), texture);
                }
            }
            return new System.Tuple<object, Object>(-1, null);
        }

        public override bool Validate(GameObject gameObject, ref List<string> errors)
        {
            // Extract the values
            System.Tuple<object, Object> value = GetValue(gameObject);
            Texture texture = value.Item2 as Texture;
            int biggestDimension = (int)value.Item1;

            // Validate the values
            bool result = biggestDimension <= TextureDimension;

            // Add a validation error if nessisary
            if (errors != null && !result) errors.Add($"{texture.name} {texture.width}x{texture.height} Texture dimentions exceeds {TextureDimension}");

            return result;
        }
    }
}
#endif