namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Defines how a tag helper will utilize its inner HTML.
    /// </summary>
    public enum ContentBehavior
    {
        /// <summary>
        /// Indicates that the tag helper will not modify its inner HTML in any way.
        /// </summary>
        None,
        /// <summary>
        /// Indicates that the tag helper wants anything within its tag builders inner HTML to be
        /// appended to the body of the generated tag.
        /// </summary>
        Append,
        /// <summary>
        /// Indicates that the tag helper will modify its HTML content.
        /// </summary>
        Modify,
        /// <summary>
        /// Indicates that the tag helper wants anything within its tag builders inner HTML to be
        /// prepended to the body of the generated tag.
        /// </summary>
        Prepend,
        /// <summary>
        /// Indicates that the tag helper wants anything within its tag builders inner HTML to
        /// replace any HTML inside of it.
        /// </summary>
        Replace,
    }
}