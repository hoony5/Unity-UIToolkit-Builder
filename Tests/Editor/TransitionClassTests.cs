using NUnit.Framework;
using UIToolkitTransitions;

namespace UIToolkitTransitions.Tests.Editor
{
    public class TransitionClassTests
    {
        [Test]
        public void NewInstance_HasSafeDefaults()
        {
            var styleClass = new TransitionClass();

            Assert.AreEqual(string.Empty, styleClass.StyleName);
            Assert.AreEqual(string.Empty, styleClass.SwappedClass);
            Assert.IsFalse(styleClass.IsTriggerStyle);
            Assert.IsTrue(styleClass.IsTriggerStyleOnStart);
        }
    }
}
