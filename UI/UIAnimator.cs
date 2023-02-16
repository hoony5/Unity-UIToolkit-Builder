using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(TransitionDataController))]
public class UIAnimator : MonoBehaviour
{
   public TransitionDataController dataController;

   private void OnEnable()
   {
      dataController ??= GetComponent<TransitionDataController>();
   }
#if  UNITY_EDITOR
   public void PlayTest()
   {
      OnToggle("root",false);
   }
   public void ReversePlayTest()
   {
      OnToggle("root",true);
   }
#endif
   public void Play(string visualElementName)
   {
      OnToggle(visualElementName, false);
   }
   public void ReversePlay(string visualElementName)
   {
      OnToggle(visualElementName, true);
   }

   public void OnUpdateStyle()
   {
      dataController.Release();
      dataController.Init();
   }

   private void AddClassToList(string visualElementName)
   {
      dataController.AddAnimatedClassList(visualElementName);
   }
   private void RemoveFromClassList(string visualElementName)
   {
      dataController.RemoveAnimatedFromClassList(visualElementName);
   }
   
   public void OnToggle(string elementName, bool setActive)
   {
      if (setActive)
         AddClassToList(elementName);
      else
         RemoveFromClassList(elementName);
   }
}
