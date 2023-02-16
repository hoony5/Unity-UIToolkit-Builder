using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "newTransitionData", menuName = "ScriptableObject/VisualElement/TransitionStyleClassNames", order = 0)]
public class TransitionData : ScriptableObject
{
    [SerializeField] public StyleSheet styleSheet;
    /// <summary>
    /// 트랜지션 대상 패널 이름
    /// </summary>
    [SerializeField] public string[] transitedPanelNames;

    [SerializeField] public TransitionClass[] styleClasses;
}
