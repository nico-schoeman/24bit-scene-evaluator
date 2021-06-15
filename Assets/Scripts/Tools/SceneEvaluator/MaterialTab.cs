using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

#if (UNITY_EDITOR)
namespace Tools
{
    /// <summary>
    /// This class defines the Material tab and it's checks as well as draws the tab specific GUI parts
    /// </summary>
    public class MaterialTab : CatagoryTabBase
    {
        // The user defined criteria options are stored here

        public override void Draw()
        {
            base.Draw();
            // Here we draw the GUI for the filter options
            
        }

        public override void Scan()
        {
            criteriaMatches = new List<ListEntry>();

            foreach (GameObject gameObject in GetGameObjects())
            {
            }
        }
    }
}
#endif