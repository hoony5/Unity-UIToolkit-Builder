# UI Toolkit Transitions

Data-driven USS class transitions for Unity UI Toolkit (Unity 6 / 6000.x).

Instead of animating UI in code, this package toggles USS classes on named
visual elements. The USS `transition-*` properties do the actual animating,
and all wiring lives in ScriptableObjects you can author in the Inspector.

## Package layout

| Folder | Contents |
| --- | --- |
| `Runtime/` | `UIAnimator` facade, `TransitionDataController`, `TransitionData`, `TransitionClass`, `TransitionDataContainer` |
| `Editor/` | UI Toolkit inspectors, USS/UXML parsers, model/controller code generator |
| `Samples~/` | Basic Transition sample: PanelSettings, USS, UXML, transition assets, prefabs |
| `Tests/` | EditMode tests for the parsers and data defaults |

## Concepts

- **TransitionData** (ScriptableObject): one transition definition.
  - `styleSheet`: USS asset that contains the animation classes. Added to the
    target elements at init.
  - `transitedPanelNames`: names of the visual elements the transition applies to.
  - `styleClasses`: ordered list of `TransitionClass` entries.
- **TransitionClass**: one USS class entry.
  - `Animation Class` (isTriggerStyle): toggled by `Play`/`ReversePlay`.
  - `Swapped Class`: counterpart class interchanged with the animation class.
  - `Start Awake Class` (isTriggerStyleOnStart): applied automatically at init.
- **TransitionDataContainer** (ScriptableObject): named group of
  TransitionData assets. Assign containers to a controller to inject many
  transitions at once (e.g. one container per screen).
- **UIAnimator / TransitionDataController**: MonoBehaviour pair placed next
  to a `UIDocument`. The controller resolves elements by name, applies style
  sheets and toggles classes. The animator is the public play API.

## Playing transitions

```csharp
using UIToolkitTransitions;

public class IntroFlow : MonoBehaviour
{
    [SerializeField] private UIAnimator uiAnimator;

    public async void ShowPanel()
    {
        await uiAnimator.PlayAsync("main-panel");
        await uiAnimator.PlayAsync("content-list");
    }

    public void HidePanel()
    {
        uiAnimator.ReversePlay("main-panel");
    }
}
```

- `Play(name)` / `ReversePlay(name)` toggle the animation classes immediately.
- `PlayAsync(name)` / `ReversePlayAsync(name)` return a `Task` that completes
  on the element's `TransitionEndEvent`. Pass a `CancellationToken` as a
  safety net: if the element's USS runs no transition, the end event never
  fires and the task would otherwise stay pending.
- Registered `TransitionStartEvent` / `TransitionEndEvent` callbacks are
  forwarded to the `onPlayStart` / `onPlayEnd` UnityEvents on the controller.

## Naming convention

Elements whose name contains `___` are ignored by the editor tooling
(panel-name popup and code generator). Use the prefix for structural
elements you never want to animate, e.g. `___background`.

## USS notes

- `display` cannot be transitioned. Hide elements with
  `visibility: hidden` + `opacity: 0` (see `.hidden` / `.show` in the sample USS).
- Each animation class should set `transition-duration` (and optionally
  `transition-delay` / `transition-timing-function`).

## Editor tools

- **TransitionData inspector**: pick target element names from a popup filled
  by scanning the assigned UXML, and animation classes from a dropdown fed by
  parsing the assigned USS.
- **UxmlToScript** (editor-time Script Generator): generate a Model
  MonoBehaviour (cached `Q<T>` queries) and a Controller MonoBehaviour (event
  registration stubs) from a UXML asset, and instance them into the scene.
- **USSReader**: lists the class selectors of a USS asset.

## Source generator (compile-time bindings)

The package ships a Roslyn source generator
(`Plugins/UIToolkitTransitions.SourceGenerators.dll`, labelled
`RoslynAnalyzer`) as the modern alternative to the editor-time
`UxmlToScript` generator. Declare a partial class and its element fields;
the bindings are generated at compile time with full type checking:

```csharp
using UIToolkitTransitions;
using UnityEngine;
using UnityEngine.UIElements;

[GenerateUiBindings]
public partial class InventoryView : MonoBehaviour
{
    public UIDocument uiDocument;

    [UiElement("btn-close")] private Button closeButton;  // explicit UXML name
    [UiElement] private Toggle muteToggle;                // name = field name
    [UiElement] private Slider volumeSlider;

    private void Start()
    {
        if (uiDocument.rootVisualElement is not null)
        {
            BindElements(uiDocument.rootVisualElement);   // generated
            RegisterCallbacks();                          // generated
        }
    }

    partial void OnCloseButtonClicked() => Destroy(gameObject);
    partial void OnMuteToggleValueChanged(ChangeEvent<bool> evt) { }
    partial void OnVolumeSliderValueChanged(ChangeEvent<float> evt) { }
}
```

Generated members:

| Member | Purpose |
| --- | --- |
| `BindElements(VisualElement root)` | cached `root.Q<T>(name)` per `[UiElement]` field |
| `InitSuccess` | true after `BindElements` finished |
| `OnElementsBound()` | partial hook called at the end of `BindElements` |
| `RegisterCallbacks()` / `UnregisterCallbacks()` | `clicked` for Button-derived fields, `RegisterValueChangedCallback` for every `INotifyValueChanged<T>` field |
| `On<Field>Clicked()` / `On<Field>ValueChanged(ChangeEvent<T> evt)` | partial hooks you implement |

The payload type `T` of the value-change hooks is inferred from the field's
`INotifyValueChanged<T>` implementation, so new control types work without
generator changes. Diagnostics: `UTTSG001` (class is not partial),
`UTTSG002` (field type is not a VisualElement).

The generator source lives in `SourceGenerator~/` (rebuild instructions in
its README). The editor-time `UxmlToScript` workflow stays available when
you prefer generating editable scripts from a UXML asset.
