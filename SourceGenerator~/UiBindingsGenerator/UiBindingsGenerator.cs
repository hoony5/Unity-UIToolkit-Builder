using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UIToolkitTransitions.SourceGenerators
{
    /// <summary>
    /// Emits compile-time UI Toolkit bindings for partial classes marked with
    /// [GenerateUiBindings]: cached Q<T> queries for every [UiElement] field,
    /// callback registration and partial hook methods.
    /// </summary>
    [Generator]
    public sealed class UiBindingsGenerator : IIncrementalGenerator
    {
        private const string GenerateUiBindingsAttributeName = "UIToolkitTransitions.GenerateUiBindingsAttribute";
        private const string UiElementAttributeName = "UIToolkitTransitions.UiElementAttribute";
        private const string UiElementsNamespace = "UnityEngine.UIElements";
        private const string VisualElementMetadataName = "UnityEngine.UIElements.VisualElement";

        private static readonly DiagnosticDescriptor NotPartialDescriptor = new DiagnosticDescriptor(
            id: "UTTSG001",
            title: "Class must be partial",
            messageFormat: "Class '{0}' uses [GenerateUiBindings] and must be declared partial",
            category: "UIToolkitTransitions",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor NotVisualElementDescriptor = new DiagnosticDescriptor(
            id: "UTTSG002",
            title: "Field must derive from VisualElement",
            messageFormat: "Field '{0}' marked with [UiElement] must derive from UnityEngine.UIElements.VisualElement",
            category: "UIToolkitTransitions",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<UiBindingModel> models = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    GenerateUiBindingsAttributeName,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, ct) => BuildModel(ctx, ct))
                .Where(static model => model is not null);

            context.RegisterSourceOutput(models, static (spc, model) => Execute(spc, model));
        }

        private static UiBindingModel BuildModel(GeneratorAttributeSyntaxContext context, CancellationToken ct)
        {
            if (context.TargetSymbol is not INamedTypeSymbol typeSymbol) return null;

            Compilation compilation = context.SemanticModel.Compilation;
            INamedTypeSymbol visualElementType = compilation.GetTypeByMetadataName(VisualElementMetadataName);
            INamedTypeSymbol uiElementAttribute = compilation.GetTypeByMetadataName(UiElementAttributeName);

            var classSyntax = (ClassDeclarationSyntax)context.TargetNode;
            bool isPartial = classSyntax.Modifiers.Any(modifier => modifier.ValueText == "partial");

            ImmutableArray<ElementBinding>.Builder elements = ImmutableArray.CreateBuilder<ElementBinding>();

            foreach (ISymbol member in typeSymbol.GetMembers())
            {
                ct.ThrowIfCancellationRequested();

                if (member is not IFieldSymbol field) continue;

                AttributeData attribute = field.GetAttributes()
                    .FirstOrDefault(data => SymbolEqualityComparer.Default.Equals(data.AttributeClass, uiElementAttribute));
                if (attribute is null) continue;

                Location location = member.Locations.FirstOrDefault() ?? Location.None;

                if (visualElementType is null || !DerivesFrom(field.Type, visualElementType))
                {
                    elements.Add(ElementBinding.Invalid(field.Name, location));
                    continue;
                }

                string elementName = attribute.ConstructorArguments.Length > 0
                    && attribute.ConstructorArguments[0].Value is string explicitName
                    && !string.IsNullOrEmpty(explicitName)
                        ? explicitName
                        : field.Name;

                elements.Add(new ElementBinding(
                    fieldName: field.Name,
                    fieldType: field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    elementName: elementName,
                    hasClickedEvent: HasClickedEvent(field.Type),
                    valueChangedPayloadType: GetValueChangedPayloadType(field.Type),
                    isValid: true,
                    location: location));
            }

            string ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : typeSymbol.ContainingNamespace.ToDisplayString();

            return new UiBindingModel(
                ns,
                typeSymbol.Name,
                isPartial,
                elements.ToImmutable(),
                classSyntax.Identifier.GetLocation());
        }

        private static void Execute(SourceProductionContext context, UiBindingModel model)
        {
            if (!model.IsPartial)
            {
                context.ReportDiagnostic(Diagnostic.Create(NotPartialDescriptor, model.Location, model.ClassName));
                return;
            }

            var elements = new List<ElementBinding>(model.Elements.Length);
            foreach (ElementBinding element in model.Elements)
            {
                if (element.IsValid)
                {
                    elements.Add(element);
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(NotVisualElementDescriptor, element.Location, element.FieldName));
            }

            var sb = new StringBuilder(1024);
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// Generated by UIToolkitTransitions.SourceGenerators.UiBindingsGenerator.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine("#nullable disable");
            sb.AppendLine();
            sb.AppendLine("using UnityEngine.UIElements;");
            sb.AppendLine();

            string outer = string.Empty;
            string member = "    ";
            string body = "        ";

            if (!string.IsNullOrEmpty(model.Namespace))
            {
                sb.Append("namespace ").Append(model.Namespace).AppendLine();
                sb.AppendLine("{");
                outer = "    ";
                member = "        ";
                body = "            ";
            }

            sb.Append(outer).Append("partial class ").AppendLine(model.ClassName);
            sb.Append(outer).AppendLine("{");

            AppendBindings(sb, elements, member, body);
            AppendCallbacks(sb, elements, member, body);

            sb.Append(outer).AppendLine("}");

            if (!string.IsNullOrEmpty(model.Namespace))
                sb.AppendLine("}");

            string hintName = string.IsNullOrEmpty(model.Namespace)
                ? model.ClassName + ".UiBindings.g.cs"
                : model.Namespace + "." + model.ClassName + ".UiBindings.g.cs";

            context.AddSource(hintName, sb.ToString());
        }

        private static void AppendBindings(StringBuilder sb, List<ElementBinding> elements, string member, string body)
        {
            sb.Append(member).AppendLine("public bool InitSuccess { get; private set; }");
            sb.AppendLine();
            sb.Append(member).AppendLine("/// <summary>Runs the cached Q&lt;T&gt; query of every [UiElement] field against the given root.</summary>");
            sb.Append(member).AppendLine("public void BindElements(VisualElement root)");
            sb.Append(member).AppendLine("{");

            foreach (ElementBinding element in elements)
            {
                sb.Append(body)
                    .Append(element.FieldName)
                    .Append(" = root.Q<")
                    .Append(element.FieldType)
                    .Append(">(\"")
                    .Append(element.ElementName)
                    .AppendLine("\");");
            }

            sb.AppendLine();
            sb.Append(body).AppendLine("InitSuccess = true;");
            sb.Append(body).AppendLine("OnElementsBound();");
            sb.Append(member).AppendLine("}");
            sb.AppendLine();
            sb.Append(member).AppendLine("/// <summary>Called at the end of BindElements. Implement this partial method to continue initialization.</summary>");
            sb.Append(member).AppendLine("partial void OnElementsBound();");
            sb.AppendLine();
        }

        private static void AppendCallbacks(StringBuilder sb, List<ElementBinding> elements, string member, string body)
        {
            var interactive = elements.Where(element => element.HasClickedEvent || element.ValueChangedPayloadType is not null).ToList();
            if (interactive.Count == 0) return;

            foreach (ElementBinding element in interactive)
            {
                if (element.HasClickedEvent)
                    sb.Append(member).Append("private System.Action _").Append(element.FieldName).AppendLine("ClickedHandler;");
                if (element.ValueChangedPayloadType is not null)
                    sb.Append(member)
                        .Append("private EventCallback<ChangeEvent<")
                        .Append(element.ValueChangedPayloadType)
                        .Append(">> _")
                        .Append(element.FieldName)
                        .AppendLine("ValueChangedHandler;");
            }
            sb.AppendLine();

            sb.Append(member).AppendLine("public void RegisterCallbacks()");
            sb.Append(member).AppendLine("{");
            foreach (ElementBinding element in interactive)
            {
                if (element.HasClickedEvent)
                {
                    sb.Append(body).Append("_").Append(element.FieldName).Append("ClickedHandler ??= () => On")
                        .Append(Pascal(element.FieldName)).AppendLine("Clicked();");
                    sb.Append(body).Append(element.FieldName).Append(".clicked += _").Append(element.FieldName).AppendLine("ClickedHandler;");
                }
                if (element.ValueChangedPayloadType is not null)
                {
                    sb.Append(body).Append("_").Append(element.FieldName).Append("ValueChangedHandler ??= evt => On")
                        .Append(Pascal(element.FieldName)).AppendLine("ValueChanged(evt);");
                    sb.Append(body).Append(element.FieldName).Append(".RegisterValueChangedCallback(_").Append(element.FieldName).AppendLine("ValueChangedHandler);");
                }
            }
            sb.Append(member).AppendLine("}");
            sb.AppendLine();

            sb.Append(member).AppendLine("public void UnregisterCallbacks()");
            sb.Append(member).AppendLine("{");
            foreach (ElementBinding element in interactive)
            {
                if (element.HasClickedEvent)
                {
                    sb.Append(body).Append("if (_").Append(element.FieldName).Append("ClickedHandler is not null) ")
                        .Append(element.FieldName).Append(".clicked -= _").Append(element.FieldName).AppendLine("ClickedHandler;");
                }
                if (element.ValueChangedPayloadType is not null)
                {
                    sb.Append(body).Append("if (_").Append(element.FieldName).Append("ValueChangedHandler is not null) ")
                        .Append(element.FieldName).Append(".UnregisterValueChangedCallback(_").Append(element.FieldName).AppendLine("ValueChangedHandler);");
                }
            }
            sb.Append(member).AppendLine("}");
            sb.AppendLine();

            foreach (ElementBinding element in interactive)
            {
                if (element.HasClickedEvent)
                {
                    sb.Append(member).Append("partial void On").Append(Pascal(element.FieldName)).AppendLine("Clicked();");
                }
                if (element.ValueChangedPayloadType is not null)
                {
                    sb.Append(member).Append("partial void On").Append(Pascal(element.FieldName))
                        .Append("ValueChanged(ChangeEvent<").Append(element.ValueChangedPayloadType).AppendLine("> evt);");
                }
            }
        }

        private static string Pascal(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        private static bool DerivesFrom(ITypeSymbol type, INamedTypeSymbol baseType)
        {
            for (ITypeSymbol current = type; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(current, baseType)) return true;
            }
            return false;
        }

        private static bool HasClickedEvent(ITypeSymbol type)
        {
            for (ITypeSymbol current = type; current is not null; current = current.BaseType)
            {
                if (current.Name == "Button" && current.ContainingNamespace?.ToDisplayString() == UiElementsNamespace)
                    return true;
            }
            return false;
        }

        private static string GetValueChangedPayloadType(ITypeSymbol type)
        {
            foreach (INamedTypeSymbol iface in type.AllInterfaces)
            {
                if (iface.IsGenericType
                    && iface.Name == "INotifyValueChanged"
                    && iface.ContainingNamespace?.ToDisplayString() == UiElementsNamespace
                    && iface.TypeArguments.Length == 1)
                {
                    return iface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
            }
            return null;
        }
    }
}
