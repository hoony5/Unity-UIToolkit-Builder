using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIAnimator))]
public class TransitionDataController : MonoBehaviour
{
    public bool debugOn;
    public UIDocument UIDocument;
    [SerializeField] private TransitionData[] animationDatas;

    [Header("Events")]
    [Space(15)]
    public UnityEvent<TransitionStartEvent> onPlayStart;
    public UnityEvent<TransitionEndEvent> onPlayEnd;
    
    private Dictionary<string, VisualElement> visualElementContainer =
        new Dictionary<string, VisualElement> (16);
    
    private Dictionary<string, List<TransitionData>> transitionContainer =
        new Dictionary<string, List<TransitionData>>(16);
    
    private IEnumerator Start()
    {
        while (UIDocument.rootVisualElement is null)
            yield return null;
        
        Init();
    }

    private void OnDestroy()
    {
        transitionContainer.Clear();
    }

    public void Init()
    {
        foreach (TransitionData animationData in animationDatas)
        {
            foreach (string visualElementName in animationData.transitedPanelNames)
            {
                AddTransitionData(visualElementName, animationData);

                VisualElement target = UIDocument.rootVisualElement.Q(visualElementName);
                
                AddVisualElement(visualElementName, target);

                AddStyleSheet(target, animationData);
                RegisterClasses(target, animationData);
                RegisterCallbackEvent(target);
            }
        }
    }

    public void Release()
    {
        transitionContainer.Clear();
        visualElementContainer.Clear();
    }

    private void AddTransitionData(string visualElementName, TransitionData animationData)
    {
        if (transitionContainer.ContainsKey(visualElementName))
        {
            List<TransitionData> currentTransition = transitionContainer[visualElementName];

            if (currentTransition.Contains(animationData)) return;
            currentTransition.Add(animationData);
        }
        else
        {
            transitionContainer.TryAdd(visualElementName, new List<TransitionData>(8));
        }
    }

    private void AddVisualElement(string visualElementName, VisualElement visualElement)
    {
        if (visualElementContainer.ContainsKey(visualElementName)) return; 
        
        visualElementContainer.TryAdd(visualElementName, visualElement);
    }
    private void RegisterCallbackEvent(VisualElement target)
    {
        target.UnregisterCallback<TransitionStartEvent>(OnTransitionStart);
        target.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
        target.RegisterCallback<TransitionStartEvent>(OnTransitionStart);
        target.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
    }
    public void StarTest()
    {
        Debug.Log("Start");
    }
    public void EndTest()
    {
        Debug.Log("End");
    }
    private void OnTransitionStart(TransitionStartEvent evt)
    {
        onPlayStart?.Invoke(evt);
        if (debugOn)
            StarTest();
    }
    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        onPlayEnd?.Invoke(evt);
        if (debugOn)
            EndTest();
    }
    
    public void AddStyleSheet(VisualElement target, TransitionData data)
    {
        if (target.styleSheets.Contains(data.styleSheet)) return;
        target.styleSheets.Add(data.styleSheet);
    }
    public void RemoveStyleSheet(VisualElement target, TransitionData data)
    {
        if (!target.styleSheets.Contains(data.styleSheet)) return;
        target.styleSheets.Remove(data.styleSheet);
    }
    public void RegisterClasses(VisualElement target, TransitionData data)
    {
        for(var i = 0 ; i < data.styleClasses?.Length; i ++)
        {
            TransitionClass styleClass = data.styleClasses[i];
            
            if(!target.ClassListContains(styleClass.StyleName))
                target.AddToClassList(styleClass.StyleName);
        }
    }
    public void UnregisterClasses(VisualElement target, TransitionData data)
    {
        for(var i = 0 ; i < data.styleClasses?.Length; i ++)
        {
            TransitionClass styleClass = data.styleClasses[i];
            
            if(!target.ClassListContains(styleClass.StyleName))
                target.AddToClassList(styleClass.StyleName);
        }
    }
    public void AddAnimatedClassList(string elementName)
    {
        if (!transitionContainer.ContainsKey(elementName)) return;

        List<TransitionData> styles = transitionContainer[elementName];
        VisualElement target = visualElementContainer[elementName];
        
        foreach (TransitionData style in styles)
        {
            Debug.Log($"{elementName} | {style.name} | {target.name}");
            for(int i = 0 ; i < style.styleClasses?.Length; i ++)
            {
                TransitionClass styleClass = style.styleClasses[i];
                if (!styleClass.IsTriggerStyle) continue;
            
                if(debugOn)
                    Debug.Log($"Add - {styleClass.StyleName}");
                target.AddToClassList(styleClass.StyleName);
            }   
        }
    }
    public void RemoveAnimatedFromClassList(string elementName)
    {
        if (!transitionContainer.ContainsKey(elementName)) return;

        List<TransitionData> styles = transitionContainer[elementName];
        VisualElement target = visualElementContainer[elementName];

        foreach (TransitionData style in styles)
        {
            Debug.Log($"{elementName} | {style.name} | {target.name}");
            for (var i = 0; i < style.styleClasses?.Length; i++)
            {
                TransitionClass styleClass = style.styleClasses[i];

                if (!styleClass.IsTriggerStyle) continue;

                if (debugOn)
                    Debug.Log($"Remove - {styleClass.StyleName}");

                target.RemoveFromClassList(styleClass.StyleName);
            }
        }
    }
}