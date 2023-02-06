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
      DeregisterClasses();
      DeregisterEvent();
      RegisterStyleClassesAndEvent();
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
      RegisterStyleClassesAndEvent();
   }

   private void OnDestroy()
   {
      DeregisterClasses();
      DeregisterEvent();
   }

   private void SetPanels(TransitionData current)
   {
      foreach (string panelName in current.TransitedPanelNames)
      {
         current.TransitedPanel = UIDocument.rootVisualElement.Q<VisualElement>(panelName);
         
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

   private void GetClasess(TransitionData current)
   {
      current.RegisterClasses();
   }

   private void RegisterClaesses()
   {
      for (var i = 0 ; i < transitionDatas.Count; i ++)
      {
         TransitionData current = transitionDatas[i];
         
         if(current is null) continue;
         if (current.TransitedPanelNames.Length == 0) continue;

         SetPanels(current);
         
         GetClasess(current);
      }
   }

   private void DeregisterClasses()
   {
      if (transitedPanels is null) return;
      if (transitedPanels.Count == 0) return;

      foreach (VisualElement panel in transitedPanels)
      {
         panel.ClearClassList();
         panel.styleSheets.Clear();
      }
      transitedPanels.Clear();
   }
   private void RegisterStyleClassesAndEvent()
   {
      if (UIDocument is null)
      {
         Debug.LogError($"UIDocument is null");
         return;
      }

      RegisterClaesses();
      
      RegisterCallBackAndRepaint();
   }

   private void DeregisterEvent()
   {
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
