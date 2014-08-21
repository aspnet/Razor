namespace Microsoft.AspNet.Razor
{
    /// <summary>
    /// Defines how a tag helper will utilize its inner html.
    /// </summary>
    public enum ContentBehavior
    {
        /// <summary>
        /// Indicates that the tag helper will not modify its inner html in any way.
        /// </summary>
        None,
        /// <summary>
        /// Indicates that the tag helper wants anything within its tag builders inner html to be
        /// appended to the body of the generated tag.
        /// </summary>
        Append,
        /// <summary>
        /// Indicates that the tag helper will modify its html content.
        /// </summary>
        Modify,
        /// <summary>
        /// Indicates that the tag helper wants anything within its tag builders inner html to be
        /// prepended to the body of the generated tag.
        /// </summary>
        Prepend,
        /// <summary>
        /// Indicates that the tag helper wants anything within its tag builders inner html to
        /// replace any html inside of it.
        /// </summary>
        Replace,
    }
}