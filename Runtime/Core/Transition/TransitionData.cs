using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitTransitions
{
    [CreateAssetMenu(fileName = "newTransitionData", menuName = "ScriptableObject/VisualElement/TransitionStyleClassNames", order = 0)]
    public class TransitionData : ScriptableObject
    {
        private const int Capacity = 32;

        [SerializeField] public VisualTreeAsset uxml;
        [SerializeField] public StyleSheet styleSheet;

        // Editor-only cache rebuilt from the USS asset; never serialized into the .asset file.
        [NonSerialized] public List<string> styleSheetsClassNames = new List<string>(Capacity);

        /// <summary>
        /// animated Target Panel
        /// </summary>
        [SerializeField] public List<string> transitedPanelNames = new List<string>(Capacity);

        [SerializeField] public TransitionClass[] styleClasses;
    }
}
