namespace UIToolkitTransitions.Tests.Editor
{
    /// <summary>
    /// Shared UXML test input. All scanner tests read from here.
    /// </summary>
    public static class UxmlFixtures
    {
        public const string BasicDocument = @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"">
    <ui:VisualElement name=""root"" class=""basicSize"">
        <ui:Button text=""Button"" name=""btn"" />
        <ui:Toggle label=""Toggle"" name=""toggle"" />
        <ui:VisualElement name=""___ignored-panel"" />
        <ui:Label text=""unnamed element"" />
    </ui:VisualElement>
</ui:UXML>";
    }
}
