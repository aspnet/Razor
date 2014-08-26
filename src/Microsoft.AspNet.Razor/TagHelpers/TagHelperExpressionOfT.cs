namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// A generic tag helper expression that is used to generate propertly typed values.
    /// </summary>
    /// <typeparam name="TBuildType">The value type to generate.</typeparam>
    public abstract class TagHelperExpression<TBuildType> : TagHelperExpression
    {
        /// <inheritdoc />
        public TagHelperExpression()
            : base()
        {
        }

        /// <summary>
        /// Resolves the current value of the expression.
        /// </summary>
        /// <param name="context">The tag helper's mid process context.</param>
        /// <returns>The typed value of the expression.</returns>
        public abstract TBuildType Build(TagHelperContext context);
    }
}