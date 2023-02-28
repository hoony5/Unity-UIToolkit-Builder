using UnityEngine;

[System.Serializable]
public class TransitionClass
{
   [SerializeField] private string styleName;
   [SerializeField] private int _selectedStyleClassIndex;
   public string StyleName => styleName;
   [SerializeField] private bool isTriggerStyle;
   public bool IsTriggerStyle => isTriggerStyle;
   [SerializeField] private bool isTriggerStyleOnStart = true;
   public bool IsTriggerStyleOnStart => isTriggerStyleOnStart;
   [SerializeField] private string swappedClass;
   
   public string SwappedClass => swappedClass;
}
