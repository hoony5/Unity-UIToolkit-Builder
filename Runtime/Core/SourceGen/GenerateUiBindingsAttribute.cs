using System;

namespace UIToolkitTransitions
{
    /// <summary>
    /// Marks a partial MonoBehaviour as a target of the UI bindings source
    /// generator. The generator emits BindElements (cached Q&lt;T&gt; queries for
    /// every [UiElement] field), RegisterCallbacks/UnregisterCallbacks and
    /// partial hook methods that you implement.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateUiBindingsAttribute : Attribute
    {
    }
}
