using NUnit.Framework;
using UIToolkitTransitions.Editor;

namespace UIToolkitTransitions.Tests.Editor
{
    public class UssClassParserTests
    {
        [Test]
        public void ParseClassNames_ReturnsAllSelectors()
        {
            var result = UssClassParser.ParseClassNames(UssFixtures.BasicStyleSheet);

            CollectionAssert.AreEquivalent(
                new[] { "basicSize", "panel--go_right", "panel--fade_in", "panel--fade_out", "colored" },
                result);
        }

        [Test]
        public void ParseClassNames_IgnoresCommentedSelectors()
        {
            var result = UssClassParser.ParseClassNames(UssFixtures.BasicStyleSheet);

            CollectionAssert.DoesNotContain(result, "commented-out");
        }

        [Test]
        public void ParseClassNames_IgnoresDeclarationBlockValues()
        {
            var result = UssClassParser.ParseClassNames(".a { background-image: resource('notAClass'); }");

            CollectionAssert.AreEquivalent(new[] { "a" }, result);
        }

        [Test]
        public void ParseClassNames_RemovesDuplicates()
        {
            var result = UssClassParser.ParseClassNames(".dup { opacity: 0; } .dup:hover { opacity: 1; }");

            CollectionAssert.AreEquivalent(new[] { "dup" }, result);
        }

        [Test]
        public void ParseClassNames_EmptyContent_ReturnsEmpty()
        {
            Assert.IsEmpty(UssClassParser.ParseClassNames(null));
            Assert.IsEmpty(UssClassParser.ParseClassNames(string.Empty));
        }
    }
}
