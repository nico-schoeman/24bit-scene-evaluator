#if (UNITY_EDITOR)
namespace Tools
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public abstract class CriteriaBase : ICriteria
    {
        /// <summary>
        /// Gets the criteria value on the spesified game object
        /// Child classes responsable for implementation
        /// </summary>
        /// <returns>A Tuple where the first value is the criteria value and the second is the responsable object</returns>
        public abstract System.Tuple<object, Object> GetValue(GameObject gameObject);

        /// <summary>
        /// Check if value is valid accoriding to criteria implementation
        /// Validation errors can be logged
        /// </summary>
        public abstract bool Validate(GameObject gameObject, ref List<string> errors);

        /// <summary>
        /// Find all the dependencies of the provided object.
        /// ComponentTypes is a list of component types to include in the check, others will be skipped
        /// </summary>
        /// <returns>List of all the dependencies that originate from the types defined in the componentTypes list</returns>
        public static List<Object> CollectDependanciesRecursive(Object obj, List<System.Type> componentTypes, List<Object> dependencies)
        {
            // Could not get EditorUtility.CollectDependencies to work as I wanted.
            // Found https://forum.unity.com/threads/editorutility-collectdependencies-on-prefab-instances-gives-me-too-much.128948/
            // Changed to suite my needs
            dependencies.Add(obj);

            SerializedObject serializedObject = new SerializedObject(obj);
            SerializedProperty property = serializedObject.GetIterator();

            while (property.Next(true))
            {
                // Traverse only through components and their fields
                if ((property.propertyType == SerializedPropertyType.ObjectReference) &&
                    (property.objectReferenceValue != null) &&
                    (property.name != "m_PrefabParentObject") &&
                    (property.name != "m_PrefabInternal") &&
                    (property.name != "m_Father") &&
                    (property.name != "m_GameObject"))
                {
                    if (property.name == "component" && componentTypes.Contains(property.objectReferenceValue.GetType()))
                    {
                        // If the property is one of the types in the componentTypes list, then we keep on recuring through the referance's properties
                        CollectDependanciesRecursive(property.objectReferenceValue, componentTypes, dependencies);
                    }
                    else if (property.name != "component")
                    {
                        CollectDependanciesRecursive(property.objectReferenceValue, componentTypes, dependencies);
                    }
                }
            }

            return dependencies;
        }
    }
}
#endif