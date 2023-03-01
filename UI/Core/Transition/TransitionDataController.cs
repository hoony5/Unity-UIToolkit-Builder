using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                RememberTransitionData(visualElementName, animationData);

                VisualElement target = UIDocument.rootVisualElement.Q(visualElementName);

                if (target is null)
                {
                    if(transitionContainer.ContainsKey(visualElementName))
                        transitionContainer.Remove(visualElementName);
                    
                    continue;
                }
                
                RememberVisualElement(visualElementName, target);

                AddStyleSheet(target, animationData);
                RegisterClasses(target, animationData);
                RegisterCallbackEvent(target);
            }
        }
    }

    public void GetVisualElementClassList(string visualElementName)
    {
        var list = visualElementContainer[visualElementName].GetClasses().ToArray();
        foreach (string item in list)
        {
            Debug.Log($"{visualElementName} : {item}");
        }
    }

    public void Release()
    {
        foreach (KeyValuePair<string, VisualElement> pair in visualElementContainer)
        {
            VisualElement target = pair.Value;
            target.ClearClassList();
        }
        
        visualElementContainer.Clear();
        transitionContainer.Clear();
    }

    private void RememberTransitionData(string visualElementName, TransitionData animationData)
    {
        if (transitionContainer.ContainsKey(visualElementName))
        {
            List<TransitionData> currentTransition = transitionContainer[visualElementName];

            if (currentTransition.Contains(animationData)) return;
            currentTransition.Add(animationData);
        }
        else
        {
            List<TransitionData> list = new List<TransitionData>(8);
            list.Add(animationData);
            transitionContainer.TryAdd(visualElementName, list);
        }
    }

    private void RememberVisualElement(string visualElementName, VisualElement visualElement)
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
        // if not set, return
        if(target is null || data.styleSheet) return;
        
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
            
            if(!styleClass.IsTriggerStyleOnStart) continue;
            
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
    public void ToggleAnimatedClassList(string elementName)
    {
        if (!transitionContainer.ContainsKey(elementName)) return;

        List<TransitionData> styles = transitionContainer[elementName];
        VisualElement target = visualElementContainer[elementName];
        
        foreach (TransitionData style in styles)
        {
            for(int i = 0 ; i < style.styleClasses?.Length; i ++)
            {
                TransitionClass styleClass = style.styleClasses[i];
                if (!styleClass.IsTriggerStyle) continue;
            
                if(debugOn)
                    Debug.Log($"Add - {styleClass.StyleName}");
                target.ToggleInClassList(styleClass.StyleName);
                
                if (!string.IsNullOrEmpty(styleClass.SwappedClass))
                    target.ToggleInClassList(styleClass.SwappedClass);
                
                target.ToggleInClassList(styleClass.StyleName);
            }   
        }
    }
    public void AddAnimatedClassList(string elementName)
    {
        if (!transitionContainer.ContainsKey(elementName)) return;

        List<TransitionData> styles = transitionContainer[elementName];
        VisualElement target = visualElementContainer[elementName];
        
        foreach (TransitionData style in styles)
        {
            for(int i = 0 ; i < style.styleClasses?.Length; i ++)
            {
                TransitionClass styleClass = style.styleClasses[i];
                if (!styleClass.IsTriggerStyle) continue;
            
                if(debugOn)
                    Debug.Log($"Add - {styleClass.StyleName}");
                
                if(!string.IsNullOrEmpty(styleClass.SwappedClass))
                    target.RemoveFromClassList(styleClass.SwappedClass);

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
            for (var i = 0; i < style.styleClasses?.Length; i++)
            {
                TransitionClass styleClass = style.styleClasses[i];

                if (!styleClass.IsTriggerStyle) continue;

                if (debugOn)
                    Debug.Log($"Remove - {styleClass.StyleName}");

                if (!string.IsNullOrEmpty(styleClass.SwappedClass))
                    target.AddToClassList(styleClass.SwappedClass);

                target.RemoveFromClassList(styleClass.StyleName);
            }
        }
    }
}