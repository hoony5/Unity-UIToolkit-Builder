# UI Toolkit Transitions

Data-driven USS class transitions for Unity UI Toolkit.

Animate UI by toggling USS classes on named visual elements — the USS
`transition-*` properties do the animating, and every transition is authored
as a ScriptableObject in the Inspector. Targets the Unity 6 line
(`6000.0` and up).

## Install

Unity Package Manager → **Add package from git URL**:

```
https://github.com/hoony5/Unity-UIToolkit-Builder.git
```

Then import the **Basic Transition** sample from the package page in the
Package Manager (PanelSettings, USS/UXML, transition assets, prefabs).

## Quick start

1. Create/prepare a `PanelSettings` asset and a `UIDocument` in your scene.

2. Place the `UIAnimator` prefab (from the sample) next to it — it carries
   the `UIAnimator` + `TransitionDataController` pair and needs the
   `UIDocument` reference assigned.

3. Write USS classes with transition properties:

```css
.panel--go_right {
    transition-duration: 0.5s;
    translate: 100% 0;
    transition-timing-function: ease-out-elastic;
}

.panel--show {
    transition-duration: 1.5s;
    translate: 0 0;
}
```

4. Create a **TransitionData** asset
   (`Create > ScriptableObject > VisualElement > TransitionStyleClassNames`):
   - assign the USS (and optionally the UXML, used only by the editor popup),
   - pick target element names from the popup (fed by scanning the UXML),
   - add style classes from the dropdown (fed by parsing the USS) and flag
     `Animation Class` / `Swapped Class` / `Start Awake Class` per entry.

   ![transition data](https://user-images.githubusercontent.com/123732566/215468252-8258e99a-c697-4c34-a46c-a2aaad10c8e9.png)

5. Assign the TransitionData (or a **TransitionDataContainer** grouping
   several of them) to the controller, plus any `onPlayStart` / `onPlayEnd`
   listeners you need.

   ![preview](https://user-images.githubusercontent.com/123732566/222043075-7a5d088a-24e3-4a44-bfcd-6c6e9edc8e80.gif)

6. Play transitions from code:

```csharp
using UIToolkitTransitions;

public class IntroFlow : MonoBehaviour
{
    [SerializeField] private UIAnimator uiAnimator;

    public async void ShowIntro()
    {
        await uiAnimator.PlayAsync("root");      // resolves on TransitionEndEvent
        await uiAnimator.PlayAsync("title");
    }

    public void HideIntro()
    {
        uiAnimator.ReversePlay("root");          // fire and forget
    }
}
```

`PlayAsync` / `ReversePlayAsync` accept a `CancellationToken` — recommended,
because an element whose USS runs no transition never fires
`TransitionEndEvent` and the task would stay pending.

## Runtime API

| Member | Purpose |
| --- | --- |
| `UIAnimator.Play(name)` / `ReversePlay(name)` | Toggle the animation classes of the element |
| `UIAnimator.PlayAsync(name, ct)` / `ReversePlayAsync(name, ct)` | Same, but await the transition end |
| `UIAnimator.OnToggle(name, setActive)` | Manual add/remove of the animation classes |
| `TransitionDataController.Init()` / `Release()` | (Re)bind or clear all registered elements |
| `TransitionDataContainer` | Group TransitionData assets; assign groups to the controller |
| `onPlayStart` / `onPlayEnd` | UnityEvents forwarding `TransitionStartEvent` / `TransitionEndEvent` |

## Editor tools

- **TransitionData inspector** — UI Toolkit based. Target element names come
  from a popup scanning the assigned UXML; style classes come from a dropdown
  parsing the assigned USS. USS/UXML are parsed only when the reference
  changes, never per repaint.
- **Script Generator** (`UxmlToScript`) — generates a Model MonoBehaviour
  (cached `Q<T>` queries per named element) and a Controller MonoBehaviour
  (event registration + callback stubs) from a UXML asset, and can instance
  the pair with a wired `UIDocument` into the hierarchy.
- **USSReader** — lists every class selector defined in a USS asset.

## Source generator (compile-time bindings)

As a modern alternative to `UxmlToScript`, the package ships a Roslyn
source generator. Declare a partial class and `[UiElement]` fields — the
queries and callback wiring are generated at compile time, fully type-checked:

```csharp
using UIToolkitTransitions;
using UnityEngine;
using UnityEngine.UIElements;

[GenerateUiBindings]
public partial class InventoryView : MonoBehaviour
{
    [UiElement("btn-close")] private Button closeButton;
    [UiElement] private Slider volumeSlider;   // element name = field name

    partial void OnCloseButtonClicked() => Destroy(gameObject);
    partial void OnVolumeSliderValueChanged(ChangeEvent<float> evt) { }
}
```

Generated: `BindElements(root)` (cached `Q<T>` queries), `InitSuccess`,
`RegisterCallbacks()` / `UnregisterCallbacks()` (`clicked` for Button-derived
fields, value-change callbacks for every `INotifyValueChanged<T>` field) and
the `On<Field>Clicked` / `On<Field>ValueChanged` partial hooks. See the
manual (`Documentation~/index.md`) for the full member table, and
`Samples~/BasicTransition/ScriptGenerator/GeneratedModelExample.cs` for a
runnable example.

## Conventions & notes

- Elements whose name contains `___` are ignored by the editor tooling
  (popup + code generator). Use it for structural elements you never animate,
  e.g. `___background`.
- USS `display` is **not** transitionable. Hide with `visibility: hidden` +
  `opacity: 0` (see the sample `.hidden` / `.show` classes).
- Sample assets live in `Samples~/` — they are not part of your build until
  you import the sample.

## Package layout

```
Runtime/        UIAnimator, TransitionDataController, TransitionData,
                TransitionClass, TransitionDataContainer, binding attributes
                (asmdef: Runtime)
Editor/         inspectors, USS/UXML parsers, editor-time code generator
                (asmdef: Editor)
Plugins/        prebuilt Roslyn source generator DLL (RoslynAnalyzer label)
Tests/          EditMode tests for parsers and data defaults
Samples~/       Basic Transition sample
SourceGenerator~/  source of the Roslyn generator + rebuild guide
Documentation~/
```

## License

[Mozilla Public License 2.0](LICENSE)
