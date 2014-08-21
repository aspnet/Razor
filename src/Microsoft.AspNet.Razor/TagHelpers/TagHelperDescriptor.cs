using System.Collections.Generic;
using Microsoft.Internal.Web.Utils;

namespace Microsoft.AspNet.Razor
{
    /// <summary>
    /// A tag helper descriptor represents how a tag helper should be used.
    /// </summary>
    public class TagHelperDescriptor
    {
        /// <summary>
        /// Instantiates a new descriptor that is used to represent a tag helper.
        /// </summary>
        /// <param name="tagName">The tag name that the tag helper should target.</param>
        /// <param name="tagHelperName">The code class that is used to render the tag helper.</param>
        /// <param name="contentBehavior">The content <see cref="Microsoft.AspNet.Razor.ContentBehavior"/>
        /// of the tag helper.</param>
        public TagHelperDescriptor(string tagName,
                                   string tagHelperName,
                                   ContentBehavior contentBehavior)
        {
            TagName = tagName;
            TagHelperName = tagHelperName;
            ContentBehavior = contentBehavior;
            Attributes = new List<TagHelperAttributeInfo>();
        }

        /// <summary>
        /// The tag name that the tag helper should target.
        /// </summary>
        public string TagName { get; private set; }
        /// <summary>
        /// The code class that is used to render the tag helper.
        /// </summary>
        public string TagHelperName { get; private set; }
        /// <summary>
        /// The content <see cref="Microsoft.AspNet.Razor.ContentBehavior"/> of the tag helper.
        /// </summary>
        public ContentBehavior ContentBehavior { get; private set; }
        /// <summary>
        /// The list of attributes that the tag helper expects.
        /// </summary>
        public virtual List<TagHelperAttributeInfo> Attributes { get; private set; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeCombiner.Start()
                                   .Add(TagName)
                                   .Add(TagHelperName)
                                   .Add(ContentBehavior)
                                   .CombinedHash;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as TagHelperDescriptor;

            return other != null &&
                   other.TagHelperName == TagHelperName &&
                   other.TagName == TagName &&
                   other.ContentBehavior == ContentBehavior;
        }
    }
}