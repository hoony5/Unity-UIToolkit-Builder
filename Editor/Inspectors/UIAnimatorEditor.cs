using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitTransitions.Editor
{
    [CustomEditor(typeof(UIAnimator))]
    public class UIAnimatorEditor : UnityEditor.Editor
    {
        private const float TestButtonHeight = 30;
        private const float TestButtonSpacing = 10;

        public override VisualElement CreateInspectorGUI()
        {
            UIAnimator animator = (UIAnimator)target;

            var root = new VisualElement();
            root.style.paddingTop = TestButtonSpacing;

            root.Add(new PropertyField(serializedObject.FindProperty("dataController"), "Animator Controller"));
            var testModeField = new PropertyField(serializedObject.FindProperty("isTestMode"), "Test Mode");
            root.Add(testModeField);

            var testArea = new VisualElement();
            testArea.style.marginTop = TestButtonSpacing;
            testArea.Add(new PropertyField(serializedObject.FindProperty("visualElementNames")));
            testArea.Add(CreateTestButton("Play Test", () =>
            {
                foreach (string visualElementName in animator.visualElementNames)
                    animator.Play(visualElementName);
            }));
            testArea.Add(CreateTestButton("Reverse Play Test", () =>
            {
                foreach (string visualElementName in animator.visualElementNames)
                    animator.ReversePlay(visualElementName);
            }));
            testArea.Add(CreateTestButton("ResetStyleClassesList", animator.OnUpdateStyle));
            testArea.Add(CreateTestButton("Debug ClassList", () =>
            {
                foreach (string visualElementName in animator.visualElementNames)
                    animator.GetClassList(visualElementName);
            }));
            root.Add(testArea);

            root.Add(new HelpBox("Test buttons only take effect in Play Mode.", HelpBoxMessageType.Info));

            testModeField.RegisterValueChangeCallback(evt =>
                testArea.style.display = evt.changedProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None);
            testArea.style.display = serializedObject.FindProperty("isTestMode").boolValue
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            return root;
        }

        private static Button CreateTestButton(string label, System.Action onClick)
        {
            return new Button(OnClicked)
            {
                text = label,
                style = { height = TestButtonHeight, marginTop = TestButtonSpacing }
            };

            void OnClicked()
            {
                if (!EditorApplication.isPlaying)
                {
                    Debug.LogWarning("UIAnimator test buttons only work in Play Mode.");
                    return;
                }
                onClick.Invoke();
            }
        }
    }
}
