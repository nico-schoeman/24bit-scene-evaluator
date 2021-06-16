using UnityEngine;
using System.Collections.Generic;

#if (UNITY_EDITOR)
namespace Tools
{
    public interface ICriteria
    {
        System.Tuple<object, Object> GetValue(GameObject gameObject);

        bool Validate(GameObject gameObject, ref List<string> errors);
    }
}
#endif