using UnityEngine;

[System.Serializable]
public class TransitionClass
{
   [SerializeField] private string styleName;
   public string StyleName => styleName;
   [SerializeField] private bool isTriggerStyle;
   public bool IsTriggerStyle => isTriggerStyle;
}
