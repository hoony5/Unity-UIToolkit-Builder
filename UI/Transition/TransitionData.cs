using System;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "newTransitionData", menuName = "ScriptableObject/VisualElement/TransitionStyleClassNames", order = 0)]
public class TransitionData : ScriptableObject
{
    [SerializeField] private bool debugOn;
    [SerializeField] private StyleSheet styleSheet;
    /// <summary>
    /// 트랜지션 대상 패널 캐쉬데이터
    /// </summary>
    [NonSerialized] public VisualElement TransitedPanel;
    private VisualElement _transitedPanel;
    /// <summary>
    /// 트랜지션 대상 패널 이름
    /// </summary>
    [SerializeField] private string[] transitedPanelNames;
    public string[] TransitedPanelNames => transitedPanelNames;

    [SerializeField] private TransitionClass[] styleClasses;
    public TransitionClass[] StyleClasses;

    private void OnEnable()
    {
        TransitedPanel = _transitedPanel;
    }

    public void RegisterClasses()
    {
        if(!TransitedPanel.styleSheets.Contains(styleSheet))
            TransitedPanel.styleSheets.Add(styleSheet);

        for(var i = 0 ; i < styleClasses?.Length; i ++)
        {
            TransitionClass styleClass = styleClasses[i];
            
            if(!TransitedPanel.ClassListContains(styleClass.StyleName))
                TransitedPanel.AddToClassList(styleClass.StyleName);
        }
    }

    /// <summary>
    /// 트랜지션 스타일이 없으면 추가 한다.
    /// </summary>
    public void AddClassList()
    {
        for(var i = 0 ; i < styleClasses?.Length; i ++)
        {
            TransitionClass styleClass = styleClasses[i];
            if (!styleClass.IsTriggerStyle) continue;
            if (TransitedPanel.ClassListContains(styleClass.StyleName)) continue;
            
            if(debugOn)
                Debug.Log($"Add - {styleClass.StyleName}");
            
            TransitedPanel.AddToClassList(styleClass.StyleName);
        }
    }

    /// <summary>
    /// 트랜지션 스타일이 있으면 제거 한다.
    /// </summary>
    public void RemoveFromClassList()
    {
        for(var i = 0 ; i < styleClasses?.Length; i ++)
        {
            TransitionClass styleClass = styleClasses[i];
            
            if (!styleClass.IsTriggerStyle) continue;
            if (!TransitedPanel.ClassListContains(styleClass.StyleName)) continue; 
            
            if(debugOn)
                Debug.Log($"Remove - {styleClass.StyleName}");
            
            TransitedPanel.RemoveFromClassList(styleClass.StyleName);
        }
    }
}