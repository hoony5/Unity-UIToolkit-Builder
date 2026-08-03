using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UIToolkitTransitions.Editor
{
    [CustomEditor(typeof(USSReader))]
    public class USSReaderEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            USSReader reader = (USSReader)target;

            var root = new VisualElement();
            root.Add(new PropertyField(serializedObject.FindProperty("uss")));
            root.Add(new PropertyField(serializedObject.FindProperty("path")));
            root.Add(new PropertyField(serializedObject.FindProperty("classNames")));

            var readButton = new Button(OnReadClicked)
            {
                text = "Read",
                style = { height = 30, marginTop = 15, marginBottom = 15 }
            };
            root.Add(readButton);

            return root;

            void OnReadClicked()
            {
                if (reader.uss is null) return;

                reader.path = AssetDatabase.GetAssetPath(reader.uss);
                reader.Read();
                EditorUtility.SetDirty(reader);
            }
        }
    }
}
