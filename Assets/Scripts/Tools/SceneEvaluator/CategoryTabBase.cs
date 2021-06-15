using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

#if (UNITY_EDITOR)
namespace Tools
{
    /// <summary>
    /// this is the base class for our catagories and it implements the shared functionality for the category tabs
    /// </summary>
    public abstract class CategoryTabBase : ICategoryTab
    {
        public string Name
        {
            get { return this.GetType().Name.Replace("Tab", ""); }
        }

        public List<ListEntry> criteriaMatches = new List<ListEntry>();

        /// <summary>
        /// This method is responsible to check the GameObjects in the scene against the criteria defined in the category controllers
        /// </summary>
        public abstract void Scan();

        /// <summary>
        /// Draws the generic GUI that the category controllers share
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