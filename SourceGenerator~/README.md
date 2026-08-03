# UIToolkitTransitions.SourceGenerators

Roslyn incremental source generator that emits compile-time UI Toolkit
bindings for partial classes marked with `[GenerateUiBindings]`.

## Rebuilding the shipped DLL

The prebuilt DLL lives at `Plugins/UIToolkitTransitions.SourceGenerators.dll`
(the Unity package content). Rebuild and overwrite it after changing this
project:

```
dotnet build SourceGenerator~/UiBindingsGenerator -c Release
copy SourceGenerator~\UiBindingsGenerator\bin\Release\netstandard2.0\UIToolkitTransitions.SourceGenerators.dll Plugins\
```

Constraints:

- Must stay on `netstandard2.0` (Unity compiler requirement for analyzers).
- `Microsoft.CodeAnalysis.CSharp` is pinned low on purpose: generators built
  against an older CodeAnalysis load on Unity's newer Roslyn, but not the
  other way around.
- The shipped DLL asset carries the `RoslynAnalyzer` label and no platform
  enabled — do not change the import settings, otherwise the DLL would be
  treated as a runtime plugin.
