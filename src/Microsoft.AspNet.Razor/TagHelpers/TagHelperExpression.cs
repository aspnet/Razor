namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// An expression is used as a property on a tag helper to determine whether the attribute
    /// is set or not via the <see cref="TagHelperExpression.IsSet"/> property.
    /// </summary>
    public class TagHelperExpression
    {
        internal static string IsSetPropertyName = "IsSet";

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperExpression"/> class with
        /// <see cref="TagHelperExpression.IsSet"/> set to false.
        /// </summary>
        public TagHelperExpression()
        {
        }

        /// <summary>
        /// Indicates whether or not the tag helper expression was set in the HTML.
        /// </summary>
        public virtual bool IsSet { get; set; }
    }
}