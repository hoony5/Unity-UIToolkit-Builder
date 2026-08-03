using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitTransitions.Editor
{
    /// <summary>
    /// Editor utility that lists the class selectors defined in a USS asset.
    /// </summary>
    public class USSReader : MonoBehaviour
    {
        private const int Capacity = 64;

        public StyleSheet uss;
        public string path;
        public List<string> classNames = new List<string>(Capacity);

        public void Read()
        {
            if (uss is null) return;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            classNames.Clear();
            classNames.AddRange(UssClassParser.ParseClassNames(File.ReadAllText(path)));
        }
    }
}
