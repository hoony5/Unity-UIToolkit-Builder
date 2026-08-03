using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace UIToolkitTransitions.SourceGenerators
{
    /// <summary>
    /// Incremental-generator snapshot of one [GenerateUiBindings] class.
    /// Location is kept for diagnostics but excluded from equality so the
    /// incremental cache survives reparses that only move text around.
    /// </summary>
    internal sealed class UiBindingModel : IEquatable<UiBindingModel>
    {
        public UiBindingModel(
            string ns,
            string className,
            bool isPartial,
            ImmutableArray<ElementBinding> elements,
            Location location)
        {
            Namespace = ns;
            ClassName = className;
            IsPartial = isPartial;
            Elements = elements;
            Location = location;
        }

        public string Namespace { get; }
        public string ClassName { get; }
        public bool IsPartial { get; }
        public ImmutableArray<ElementBinding> Elements { get; }
        public Location Location { get; }

        public bool Equals(UiBindingModel other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Namespace != other.Namespace) return false;
            if (ClassName != other.ClassName) return false;
            if (IsPartial != other.IsPartial) return false;
            if (Elements.Length != other.Elements.Length) return false;

            for (int i = 0; i < Elements.Length; i++)
            {
                if (!Elements[i].Equals(other.Elements[i])) return false;
            }

            return true;
        }

        public override bool Equals(object obj) => Equals(obj as UiBindingModel);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
                hash = hash * 31 + (ClassName?.GetHashCode() ?? 0);
                hash = hash * 31 + IsPartial.GetHashCode();
                hash = hash * 31 + Elements.Length;
                return hash;
            }
        }
    }

    internal sealed class ElementBinding : IEquatable<ElementBinding>
    {
        public ElementBinding(
            string fieldName,
            string fieldType,
            string elementName,
            bool hasClickedEvent,
            string valueChangedPayloadType,
            bool isValid,
            Location location)
        {
            FieldName = fieldName;
            FieldType = fieldType;
            ElementName = elementName;
            HasClickedEvent = hasClickedEvent;
            ValueChangedPayloadType = valueChangedPayloadType;
            IsValid = isValid;
            Location = location;
        }

        public static ElementBinding Invalid(string fieldName, Location location)
        {
            return new ElementBinding(fieldName, null, null, false, null, false, location);
        }

        public string FieldName { get; }
        public string FieldType { get; }
        public string ElementName { get; }
        public bool HasClickedEvent { get; }
        public string ValueChangedPayloadType { get; }
        public bool IsValid { get; }
        public Location Location { get; }

        public bool Equals(ElementBinding other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return FieldName == other.FieldName
                && FieldType == other.FieldType
                && ElementName == other.ElementName
                && HasClickedEvent == other.HasClickedEvent
                && ValueChangedPayloadType == other.ValueChangedPayloadType
                && IsValid == other.IsValid;
        }

        public override bool Equals(object obj) => Equals(obj as ElementBinding);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (FieldName?.GetHashCode() ?? 0);
                hash = hash * 31 + (FieldType?.GetHashCode() ?? 0);
                hash = hash * 31 + (ElementName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
