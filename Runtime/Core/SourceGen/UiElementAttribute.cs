using System;

namespace UIToolkitTransitions
{
    /// <summary>
    /// Declares a visual element field for the UI bindings source generator.
    /// The generated BindElements method assigns the field via
    /// root.Q&lt;T&gt;(ElementName). When ElementName is null the field name is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class UiElementAttribute : Attribute
    {
        public string ElementName { get; }

        public UiElementAttribute(string elementName = null)
        {
            ElementName = elementName;
        }
    }
}
