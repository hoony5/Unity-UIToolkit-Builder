using System.Collections.Generic;
using UnityEngine;

namespace UIToolkitTransitions
{
    /// <summary>
    /// Named group of <see cref="TransitionData"/> assets.
    /// Assign containers to a TransitionDataController to inject every
    /// transition inside the group at once (e.g. one container per screen).
    /// </summary>
    [CreateAssetMenu(fileName = "newTransitionDataContainer", menuName = "ScriptableObject/Transition/Container", order = 0)]
    public class TransitionDataContainer : ScriptableObject
    {
        [SerializeField] private string groupName = string.Empty;
        [SerializeField] private List<TransitionData> transitionDatas = new List<TransitionData>();

        public string GroupName => groupName;
        public IReadOnlyList<TransitionData> TransitionDatas => transitionDatas;
    }
}
