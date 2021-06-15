using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

#if (UNITY_EDITOR)
namespace Tools
{
    /// <summary>
    /// this is the base class for our catagories and it implements the shared functionality for the catagory tabs
    /// </summary>
    public abstract class CatagoryTabBase : ICatagoryTab
    {
        public string Name
        {
            get { return this.GetType().Name.Replace("Tab", ""); }
        }

        public List<ListEntry> criteriaMatches = new List<ListEntry>();

        /// <summary>
        /// This method is responsible to check the GameObjects in the scene against the criteria defined in the catagory controllers
        /// </summary>
        public abstract void Scan();

        /// <summary>
        /// Draws the generic GUI that the catagory controllers share
        /// </summary>
        public virtual void Draw()
        {
            EditorGUILayout.LabelField(Name, EditorStyles.boldLabel);
            if (GUILayout.Button("Scan"))
            {
                Scan();
                // After the validation scan we select the GameObjects that failed the checks
                Selection.objects = criteriaMatches.Select<ListEntry, GameObject>(match => { return match.gameObject; }).ToArray();
            }
        }

        /// <summary>
        /// Gets all the root GameObjects in the active scene
        /// </summary>
        public virtual GameObject[] GetGameObjects()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            GameObject[] gameObjects = scene.GetRootGameObjects();
            return gameObjects;
        }

        /// <summary>
        /// Add a new ListEntry to the matches list with the GameObject and a list of errors for that GameObject
        /// If the GameObject already exsists in the list instead just append the errors
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="errors"></param>
        public void AddCriteriaMatch(GameObject gameObject, List<string> errors)
        {
            if (criteriaMatches.Any(match => match.gameObject == gameObject))
            {
                ListEntry entry = criteriaMatches.Single(match => match.gameObject == gameObject);
                entry.validationErrors = entry.validationErrors.Union(errors).ToList();
            }
            else
            {
                criteriaMatches.Add(new ListEntry() { gameObject = gameObject, validationErrors = errors });
            }
        }

        /// <summary>
        /// Find all the dependencies of the provided object.
        /// componentTypes is a list of component types to include in the check, others will be skipped
        /// </summary>
        /// <returns>list of all the dependencies that originate from the types defined in the componentTypes list</returns>
        public List<Object> CollectDependanciesRecursive (Object obj, List<System.Type> componentTypes, Dictionary<int, Object> dependencies)
        {
            // Could not get EditorUtility.CollectDependencies to work as I wanted.
            // Found https://forum.unity.com/threads/editorutility-collectdependencies-on-prefab-instances-gives-me-too-much.128948/
            // Changed to suite my needs
            if (!dependencies.ContainsKey(obj.GetHashCode()))
            {
                dependencies.Add(obj.GetHashCode(), obj);

                SerializedObject serializedObject = new SerializedObject(obj);
                SerializedProperty property = serializedObject.GetIterator();

                while (property.Next(true))
                {
                    if ((property.propertyType == SerializedPropertyType.ObjectReference) &&
                        (property.objectReferenceValue != null) &&
                        (property.name != "m_PrefabParentObject") && // Don't follow prefabs
                        (property.name != "m_PrefabInternal") && // Don't follow prefab internals
                        (property.name != "m_GameObject")) // Don't go to other gameObjects
                    {
                        if (property.name == "component" && componentTypes.Contains(property.objectReferenceValue.GetType()))
                        {
                            // if the property is one of the types in the componentTypes list, then we keep on recuring through the referance's properties
                            CollectDependanciesRecursive(property.objectReferenceValue, componentTypes, dependencies);
                        }
                        else if (property.name != "component")
                        {
                            CollectDependanciesRecursive(property.objectReferenceValue, componentTypes, dependencies);
                        }
                    }
                };

            }

            return dependencies.Values.ToList();
        }
    }

    /// <summary>
    /// This struct is used to store the data of GameObjects that failed the validation criteria
    /// </summary>
    public struct ListEntry
    {
        public GameObject gameObject;
        public List<string> validationErrors;
        public bool foldout;
    }
}
#endif