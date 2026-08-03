using System.Collections.Generic;
using System.Xml.Linq;

namespace UIToolkitTransitions.Editor
{
    /// <summary>
    /// Named element entry found in a UXML document.
    /// </summary>
    public readonly struct UxmlElementInfo
    {
        public UxmlElementInfo(string typeName, string elementName)
        {
            TypeName = typeName;
            ElementName = elementName;
        }

        public string TypeName { get; }
        public string ElementName { get; }

        public void Deconstruct(out string typeName, out string elementName)
        {
            typeName = TypeName;
            elementName = ElementName;
        }
    }

    /// <summary>
    /// Scans named visual elements out of raw UXML text. Editor-only utility.
    /// Elements whose name contains <see cref="IgnoreElementNamePrefix"/> are skipped.
    /// </summary>
    public static class UxmlElementScanner
    {
        public const string IgnoreElementNamePrefix = "___";

        public static IReadOnlyList<UxmlElementInfo> Scan(string uxmlContent, string ignorePrefix = IgnoreElementNamePrefix)
        {
            var result = new List<UxmlElementInfo>();
            if (string.IsNullOrEmpty(uxmlContent)) return result;

            XDocument document = XDocument.Parse(uxmlContent);

            foreach (XElement element in document.Elements().Descendants())
            {
                string name = element.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(name)) continue;
                if (!string.IsNullOrEmpty(ignorePrefix) && name.Contains(ignorePrefix)) continue;

                result.Add(new UxmlElementInfo(element.Name.LocalName, name));
            }

            return result;
        }
    }
}
