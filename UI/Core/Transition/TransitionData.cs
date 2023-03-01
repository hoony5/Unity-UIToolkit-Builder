using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable, CreateAssetMenu(fileName = "newTransitionData", menuName = "ScriptableObject/VisualElement/TransitionStyleClassNames", order = 0)]
public class TransitionData : ScriptableObject
{
    private const int Capactiy = 32;
    [SerializeField] public VisualTreeAsset uxml;
    [SerializeField] public StyleSheet styleSheet;
    
    [SerializeField] public List<string> styleSheetsClassNames = new List<string>(Capactiy);
    /// <summary>
    /// animated Target Panel
    /// </summary>
    [SerializeField] public List<string> transitedPanelNames = new List<string>(Capactiy);

    [SerializeField] public TransitionClass[] styleClasses;
}
