using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitTransitions.Editor
{
    /// <summary>
    /// One row of the "Style Target VisualElement" list.
    /// Shows a Popup with names scanned from the UXML asset when one is
    /// assigned, otherwise falls back to a disabled-state TextField.
    /// </summary>
    internal sealed class PanelNameRow : VisualElement
    {
        private const float LabelWidth = 140f;

        private readonly Popup _popup;
        private readonly TextField _textField;
        private SerializedProperty _boundProperty;
        private EventCallback<ChangeEvent<int>> _popupChanged;
        private EventCallback<ChangeEvent<string>> _textChanged;

        public PanelNameRow()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            Add(new Label("Element Name") { style = { width = LabelWidth } });
            _popup = new Popup { style = { flexGrow = 1, display = DisplayStyle.None } };
            _textField = new TextField { style = { flexGrow = 1, display = DisplayStyle.None } };
            Add(_popup);
            Add(_textField);
        }

        public void Bind(SerializedProperty property, IReadOnlyList<string> choices)
        {
            Unbind();
            _boundProperty = property;

            if (choices.Count > 0)
            {
                _popup.choices = choices.ToList();
                _popup.SetValueWithoutNotify(Mathf.Max(0, _popup.choices.IndexOf(property.stringValue)));
                _popupChanged = evt => WriteChoice(evt.newValue);
                _popup.RegisterValueChangedCallback(_popupChanged);
                _popup.style.display = DisplayStyle.Flex;
            }
            else
            {
                _textField.SetValueWithoutNotify(property.stringValue);
                _textChanged = evt => WriteName(evt.newValue);
                _textField.RegisterValueChangedCallback(_textChanged);
                _textField.style.display = DisplayStyle.Flex;
            }
        }

        public void Unbind()
        {
            if (_popupChanged is not null) _popup.UnregisterValueChangedCallback(_popupChanged);
            if (_textChanged is not null) _textField.UnregisterValueChangedCallback(_textChanged);
            _popupChanged = null;
            _textChanged = null;
            _boundProperty = null;
            _popup.style.display = DisplayStyle.None;
            _textField.style.display = DisplayStyle.None;
        }

        private void WriteChoice(int choiceIndex)
        {
            if (_boundProperty is null || choiceIndex < 0 || choiceIndex >= _popup.choices.Count) return;

            _boundProperty.stringValue = _popup.choices[choiceIndex];
            _boundProperty.serializedObject.ApplyModifiedProperties();
        }

        private void WriteName(string elementName)
        {
            if (_boundProperty is null) return;

            _boundProperty.stringValue = elementName;
            _boundProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// One row of the "Style Classes" list.
    /// styleName is a DropdownField fed from the parsed USS classes when a
    /// StyleSheet is assigned, otherwise a free-form TextField.
    /// </summary>
    internal sealed class StyleClassRow : VisualElement
    {
        private const float ToggleSpacing = 12f;

        private readonly DropdownField _styleNameDropdown;
        private readonly TextField _styleNameField;
        private readonly Toggle _isTriggerToggle;
        private readonly Toggle _isTriggerOnStartToggle;
        private readonly TextField _swappedField;

        private SerializedProperty _styleNameProperty;
        private SerializedProperty _isTriggerProperty;
        private SerializedProperty _isTriggerOnStartProperty;
        private SerializedProperty _swappedProperty;

        private EventCallback<ChangeEvent<string>> _styleNameDropdownChanged;
        private EventCallback<ChangeEvent<string>> _styleNameFieldChanged;
        private EventCallback<ChangeEvent<bool>> _isTriggerChanged;
        private EventCallback<ChangeEvent<bool>> _isTriggerOnStartChanged;
        private EventCallback<ChangeEvent<string>> _swappedChanged;

        public StyleClassRow()
        {
            var firstLine = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
            };
            _styleNameDropdown = new DropdownField { style = { flexGrow = 1, display = DisplayStyle.None } };
            _styleNameField = new TextField { style = { flexGrow = 1, display = DisplayStyle.None } };
            firstLine.Add(_styleNameDropdown);
            firstLine.Add(_styleNameField);

            var secondLine = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
            };
            _isTriggerToggle = new Toggle("Animation Class");
            _isTriggerOnStartToggle = new Toggle("Start Awake Class") { style = { marginLeft = ToggleSpacing } };
            _swappedField = new TextField("Swapped Class") { style = { flexGrow = 1, marginLeft = ToggleSpacing } };
            secondLine.Add(_isTriggerToggle);
            secondLine.Add(_isTriggerOnStartToggle);
            secondLine.Add(_swappedField);

            Add(firstLine);
            Add(secondLine);
        }

        public void Bind(SerializedProperty elementProperty, IReadOnlyList<string> classChoices)
        {
            Unbind();

            _styleNameProperty = elementProperty.FindPropertyRelative("styleName");
            _isTriggerProperty = elementProperty.FindPropertyRelative("isTriggerStyle");
            _isTriggerOnStartProperty = elementProperty.FindPropertyRelative("isTriggerStyleOnStart");
            _swappedProperty = elementProperty.FindPropertyRelative("swappedClass");

            if (classChoices.Count > 0)
            {
                _styleNameDropdown.choices = classChoices.ToList();
                _styleNameDropdown.SetValueWithoutNotify(_styleNameProperty.stringValue);
                _styleNameDropdownChanged = evt => WriteString(_styleNameProperty, evt.newValue);
                _styleNameDropdown.RegisterValueChangedCallback(_styleNameDropdownChanged);
                _styleNameDropdown.style.display = DisplayStyle.Flex;
            }
            else
            {
                _styleNameField.SetValueWithoutNotify(_styleNameProperty.stringValue);
                _styleNameFieldChanged = evt => WriteString(_styleNameProperty, evt.newValue);
                _styleNameField.RegisterValueChangedCallback(_styleNameFieldChanged);
                _styleNameField.style.display = DisplayStyle.Flex;
            }

            _isTriggerToggle.SetValueWithoutNotify(_isTriggerProperty.boolValue);
            _isTriggerChanged = evt => WriteBool(_isTriggerProperty, evt.newValue);
            _isTriggerToggle.RegisterValueChangedCallback(_isTriggerChanged);

            _isTriggerOnStartToggle.SetValueWithoutNotify(_isTriggerOnStartProperty.boolValue);
            _isTriggerOnStartChanged = evt => WriteBool(_isTriggerOnStartProperty, evt.newValue);
            _isTriggerOnStartToggle.RegisterValueChangedCallback(_isTriggerOnStartChanged);

            _swappedField.SetValueWithoutNotify(_swappedProperty.stringValue);
            _swappedChanged = evt => WriteString(_swappedProperty, evt.newValue);
            _swappedField.RegisterValueChangedCallback(_swappedChanged);
        }

        public void Unbind()
        {
            if (_styleNameDropdownChanged is not null)
                _styleNameDropdown.UnregisterValueChangedCallback(_styleNameDropdownChanged);
            if (_styleNameFieldChanged is not null)
                _styleNameField.UnregisterValueChangedCallback(_styleNameFieldChanged);
            if (_isTriggerChanged is not null)
                _isTriggerToggle.UnregisterValueChangedCallback(_isTriggerChanged);
            if (_isTriggerOnStartChanged is not null)
                _isTriggerOnStartToggle.UnregisterValueChangedCallback(_isTriggerOnStartChanged);
            if (_swappedChanged is not null)
                _swappedField.UnregisterValueChangedCallback(_swappedChanged);

            _styleNameDropdownChanged = null;
            _styleNameFieldChanged = null;
            _isTriggerChanged = null;
            _isTriggerOnStartChanged = null;
            _swappedChanged = null;

            _styleNameProperty = null;
            _isTriggerProperty = null;
            _isTriggerOnStartProperty = null;
            _swappedProperty = null;

            _styleNameDropdown.style.display = DisplayStyle.None;
            _styleNameField.style.display = DisplayStyle.None;
        }

        private static void WriteString(SerializedProperty property, string value)
        {
            property.stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void WriteBool(SerializedProperty property, bool value)
        {
            property.boolValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
