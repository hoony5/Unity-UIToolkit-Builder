using System.Linq;
using NUnit.Framework;
using UIToolkitTransitions.Editor;

namespace UIToolkitTransitions.Tests.Editor
{
    public class UxmlElementScannerTests
    {
        [Test]
        public void Scan_ReturnsNamedElements()
        {
            var result = UxmlElementScanner.Scan(UxmlFixtures.BasicDocument);

            CollectionAssert.AreEquivalent(
                new[] { "root", "btn", "toggle" },
                result.Select(info => info.ElementName));
        }

        [Test]
        public void Scan_MapsElementTypes()
        {
            var result = UxmlElementScanner.Scan(UxmlFixtures.BasicDocument);

            Assert.AreEqual("Button", result.First(info => info.ElementName == "btn").TypeName);
            Assert.AreEqual("Toggle", result.First(info => info.ElementName == "toggle").TypeName);
        }

        [Test]
        public void Scan_SkipsIgnoredPrefixAndUnnamedElements()
        {
            var result = UxmlElementScanner.Scan(UxmlFixtures.BasicDocument);

            CollectionAssert.DoesNotContain(
                result.Select(info => info.ElementName).ToList(),
                "___ignored-panel");
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void Scan_CustomIgnorePrefix()
        {
            var result = UxmlElementScanner.Scan(UxmlFixtures.BasicDocument, "toggle");

            CollectionAssert.DoesNotContain(
                result.Select(info => info.ElementName).ToList(),
                "toggle");
        }

        [Test]
        public void Scan_EmptyContent_ReturnsEmpty()
        {
            Assert.IsEmpty(UxmlElementScanner.Scan(null));
            Assert.IsEmpty(UxmlElementScanner.Scan(string.Empty));
        }
    }
}
