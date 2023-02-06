using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UIAnimator : MonoBehaviour
{
   public UIDocument UIDocument;
   [SerializeField] private List<TransitionData> transitionDatas = new List<TransitionData>(32);
   private List<VisualElement> transitedPanels = new List<VisualElement>(16);
   [Header("Events")]
   [Space(15)]
   public UnityEvent<TransitionStartEvent> onPlayStart;
   public UnityEvent<TransitionEndEvent> onPlayEnd;

#if  UNITY_EDITOR
   public void InitEvt()
   {
      RegisterEvent();
   }
   public void StarTest()
   {
      Debug.Log("Start");
   }
   public void EndTest()
   {
      Debug.Log("End");
   }
#endif
   public void Play()
   {
      OnUpdateStyle();
      OnToggle(false);
   }
   public void ReversePlay()
   {
      OnUpdateStyle();
      OnToggle(true);
   }
   public void OnUpdateStyle()
   {
      DeregisterEvent();
      RegisterEvent();
   }

   public void AddStyle(TransitionData data)
   {
      if (transitionDatas.Contains(data)) return;
      transitionDatas.Add(data);
      OnUpdateStyle();
   }

   public void RemoveStyle(TransitionData data)
   {
      if (!transitionDatas.Contains(data)) return;
      transitionDatas.Remove(data);
      OnUpdateStyle();
   }
   private void Start()
   {
      RegisterEvent();
   }

   private void OnDestroy()
   {
      DeregisterEvent();
   }

   private void SavePanels(TransitionData current)
   {
      foreach (string panelName in current.TransitedPanelNames)
      {
         current.TransitedPanel ??= UIDocument.rootVisualElement.Q<VisualElement>(panelName);
         
         if(!transitedPanels.Contains(current.TransitedPanel))
            transitedPanels.Add(current.TransitedPanel);
      }
   }
   private void RegisterCallBackAndRepaint()
   {
      if (transitedPanels is null) return;
      if (transitedPanels.Count == 0) return;

      for (int i = 0; i < transitedPanels.Count; i++)
      {
         VisualElement current = transitedPanels[i];
         current.UnregisterCallback<TransitionStartEvent>(OnTransitionStart);
         current.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
         current.RegisterCallback<TransitionStartEvent>(OnTransitionStart);
         current.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
         
         current.MarkDirtyRepaint();
      }
   }

   private void RegisterClasses(TransitionData current)
   {
      current.RegisterClasses();
   }

   private void DeregisterClasses()
   {
      if (transitedPanels is null) return;
      if (transitedPanels.Count == 0) return;

      for (int i = 0; i < transitedPanels.Count; i++)
      {
         transitedPanels[i].ClearClassList();
      }
   }
   private void RegisterEvent()
   {
      if (UIDocument is null)
      {
         Debug.LogError($"UIDocument is null");
         return;
      }

      if (transitionDatas is null || transitionDatas.Count == 0) return;
      
      transitedPanels.Clear();
      
      for (var i = 0 ; i < transitionDatas.Count; i ++)
      {
         TransitionData current = transitionDatas[i];
         
         if(current is null) continue;
         if (current.TransitedPanelNames.Length == 0) continue;

         SavePanels(current);
         
         RegisterClasses(current);
      }
      
      RegisterCallBackAndRepaint();
   }

   private void DeregisterEvent()
   {
      DeregisterClasses();
      if (transitedPanels is null) return;
      if (transitedPanels.Count == 0) return;

      for (int i = 0; i < transitedPanels.Count; i++)
      {
         VisualElement current = transitedPanels[i];
         current.UnregisterCallback<TransitionStartEvent>(OnTransitionStart);
         current.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
      }
   }

   private void OnTransitionStart(TransitionStartEvent evt)
   {
      onPlayStart?.Invoke(evt);
   }
   private void OnTransitionEnd(TransitionEndEvent evt)
   {
      onPlayEnd?.Invoke(evt);
   }
   
   public void OnToggle(bool setActive)
   {
      if (UIDocument is null)
      {
         Debug.LogError($"UIDocument is null");
         return;
      }
      
      foreach (TransitionData current in transitionDatas)
      {
         if(setActive)
            current.AddClassList();
         else
            current.RemoveFromClassList();
      }
   }
}
