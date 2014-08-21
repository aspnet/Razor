namespace Microsoft.AspNet.Razor
{
    /// <summary>
    /// A metadata class used to communicate the functionality of tag helper attributes.
    /// </summary>
    public class TagHelperAttributeInfo
    {
        /// <summary>
        /// Instantiates a new tag helper attribute info class.
        /// </summary>
        /// <param name="attributeName">The html attribute name mapping.</param>
        /// <param name="attributePropertyName">The property name that corresponds to the html 
        /// attribute name.</param>
        /// <param name="acceptRazorCode">Whether or not the attribute accepts raw Razor code.</param>
        /// <param name="codeGenerator">The <see cref="TagHelperAttributeCodeGenerator"/> that is used to
        /// generate the code to instantiate the <paramref name="attributePropertyName"/> property on the
        /// tag helper.</param>
        public TagHelperAttributeInfo(string attributeName,
                                      string attributePropertyName,
                                      bool acceptRazorCode,
                                      TagHelperAttributeCodeGenerator codeGenerator)
        {
            AttributeName = attributeName;
            AttributePropertyName = attributePropertyName;
            AcceptsRazorCode = acceptRazorCode;
            CodeGenerator = codeGenerator;
        }

        /// <summary>
        /// The html attribute name mapping.
        /// </summary>
        public string AttributeName { get; private set; }
        /// <summary>
        /// The property name that corresponds to the html attribute name.
        /// </summary>
        public string AttributePropertyName { get; private set; }
        /// <summary>
        /// Indicates whether or not the attribute accepts raw Razor code.
        /// </summary>
        public bool AcceptsRazorCode { get; private set; }
        /// <summary>
        /// The <see cref="TagHelperAttributeCodeGenerator"/> that is used to generate the code to instantiate 
        /// the <paramref name="attributePropertyName"/> property on the tag helper.
        /// </summary>
        public TagHelperAttributeCodeGenerator CodeGenerator { get; private set; }
    }
}