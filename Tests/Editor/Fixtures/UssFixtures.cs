namespace UIToolkitTransitions.Tests.Editor
{
    /// <summary>
    /// Shared USS test input. All parser tests read from here.
    /// </summary>
    public static class UssFixtures
    {
        public const string BasicStyleSheet = @"
.basicSize {
    width: 256px;
    height: 256px;
}

.panel--go_right {
    transition-duration: 0.5s;
    translate: 100% 0;
}

.panel--fade_in, .panel--fade_out {
    transition-duration: 2s;
}

.colored:checked:hover {
    background-color: rgb(100, 100, 100);
}

/* .commented-out must be ignored */
";
    }
}
