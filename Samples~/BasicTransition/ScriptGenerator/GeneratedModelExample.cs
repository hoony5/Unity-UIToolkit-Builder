using System.Collections;
using UIToolkitTransitions;
using UnityEngine;
using UnityEngine.UIElements;

// Compile-time UI bindings via the UIToolkitTransitions source generator.
// BindElements / RegisterCallbacks / UnregisterCallbacks are generated from
// the [UiElement] fields below; implement the partial hooks you need.
// Compare with FooModel/FooModelController, which the editor-time
// UxmlToScript generator produces from the UXML asset.
[GenerateUiBindings]
public partial class GeneratedModelExample : MonoBehaviour
{
    public UIDocument uiDocument;

    [UiElement("root")] private VisualElement rootElement;
    [UiElement("btn")] private Button button;
    [UiElement("toggle")] private Toggle toggle;
    [UiElement] private ListView listView;

    private void OnEnable()
    {
        StartCoroutine(InitRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (InitSuccess)
            UnregisterCallbacks();
    }

    private IEnumerator InitRoutine()
    {
        while (uiDocument is null || uiDocument.rootVisualElement is null)
            yield return null;

        BindElements(uiDocument.rootVisualElement);
        RegisterCallbacks();
    }

    partial void OnButtonClicked()
    {
        Debug.Log("GeneratedModelExample: button clicked");
    }

    partial void OnToggleValueChanged(ChangeEvent<bool> evt)
    {
        Debug.Log($"GeneratedModelExample: toggle changed to {evt.newValue}");
    }
}
