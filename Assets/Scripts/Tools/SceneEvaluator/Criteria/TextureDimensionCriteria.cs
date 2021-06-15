using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)
namespace Tools {
    public class TextureDimensionCriteria : CriteriaBase
    {
        public int textureDimension;

        public override System.Tuple<object, Object> GetValue(GameObject gameObject)
        {
            foreach (Object obj in CollectDependanciesRecursive(gameObject, new List<System.Type> { typeof(MeshRenderer), typeof(SkinnedMeshRenderer) }, new List<Object>()))
            {
                if (obj is Texture)
                {
                    Texture texture = obj as Texture;
                    return new System.Tuple<object, Object> (Mathf.Max(texture.width, texture.height), texture);
                }
            }
            return new System.Tuple<object, Object>(-1, null);
        }

        public override bool Validate(GameObject gameObject, ref List<string> errors)
        {
            System.Tuple<object, Object> value = GetValue(gameObject);
            Texture texture = value.Item2 as Texture;
            int biggestDimension = (int)value.Item1;
            bool result;
            if (biggestDimension > textureDimension)
                result = false;
            else
                result = true;

            if (errors != null && !result) errors.Add($"{texture.name} {texture.width}x{texture.height} Texture dimentions exceeds {textureDimension}");

            return result;
        }
    }
}
#endif