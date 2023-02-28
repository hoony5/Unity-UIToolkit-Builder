using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newTransitionDataContainer", menuName = "ScriptableObject/Transition/Container", order = 0)]
public class TransitionDataContainer : ScriptableObject
{
    public string index;
    public List<TransitionData> transitionDatas;
}