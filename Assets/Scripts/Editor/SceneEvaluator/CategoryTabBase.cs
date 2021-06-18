#if (UNITY_EDITOR)
namespace Tools
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    /// <summary>
    /// This is the base class for our catagories and it implements the shared functionality for the category tabs
    /// </summary>
    public abstract class CategoryTabBase : ICategoryTab
    {
        public List<ListEntry> CriteriaMatches { get; set; } = new List<ListEntry>();

        /// <summary>
        /// Gets the name of the current type with "Tab" removed
        /// </summary>
        public string GetName()
        {
            return this.GetType().Name.Replace("Tab", string.Empty);
        }

        /// <summary>
        /// This method is responsible to check the GameObjects in the scene against the criteria defined in the category controllers
        /// </summary>
        public abstract void Scan();

        /// <summary>
        /// Draws the generic GUI that the category controllers share
        /// </summary>
        public virtual void Draw()
        {
            EditorGUILayout.LabelField(GetName(), EditorStyles.boldLabel);
            if (GUILayout.Button("Scan"))
            {
                Scan();
                // After the validation scan we select the GameObjects that failed the checks
                Selection.objects = CriteriaMatches.Select<ListEntry, GameObject>(match => match.gameObject).ToArray();
            }
        }

        /// <summary>
        /// Gets all the root GameObjects in the active scene
        /// </summary>
        public virtual GameObject[] GetGameObjects()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            return scene.GetRootGameObjects();
        }

        /// <summary>
        /// Add a new ListEntry to the matches list with the GameObject and a list of errors for that GameObject
        /// If the GameObject already exsists in the list instead just append the errors
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="errors"></param>
        public void AddCriteriaMatch(GameObject gameObject, List<string> errors)
        {
            if (CriteriaMatches.Any(match => match.gameObject == gameObject))
            {
                ListEntry entry = CriteriaMatches.Single(match => match.gameObject == gameObject);
                entry.validationErrors = entry.validationErrors.Union(errors).ToList();
            }
            else
            {
                CriteriaMatches.Add(new ListEntry() { gameObject = gameObject, validationErrors = errors });
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