using UnityEngine;

namespace UIToolkitTransitions
{
    [System.Serializable]
    public class TransitionClass
    {
        [SerializeField] private string styleName = string.Empty;
        [SerializeField] private int _selectedStyleClassIndex;
        [SerializeField] private bool isTriggerStyle;
        [SerializeField] private bool isTriggerStyleOnStart = true;
        [SerializeField] private string swappedClass = string.Empty;

        public string StyleName => styleName;
        public bool IsTriggerStyle => isTriggerStyle;
        public bool IsTriggerStyleOnStart => isTriggerStyleOnStart;
        public string SwappedClass => swappedClass;
    }
}
