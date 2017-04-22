using System;
using System.ComponentModel;
using System.Resources;

namespace Common.Strings
{
    /// <summary>
    /// Attribute for localized description: allow to get translated description with language parameter
    /// </summary>
    /// <seealso cref="T:System.ComponentModel.DescriptionAttribute"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "not needed")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string resourceKey;
        private readonly string keySuffix;
        private readonly ResourceManager resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedDescriptionAttribute" /> class.
        /// Will read the string defined by the key from the resource type.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="keySuffix">The key suffix, common for all languages.</param>
        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType, string keySuffix = null)
        {
            resource = new ResourceManager(resourceType);
            this.resourceKey = resourceKey;
            this.keySuffix = keySuffix;
        }

        /// <summary>
        /// Gets the description stored in this attribute, reading from the resource!
        /// </summary>
        public override string Description
        {
            get
            {
                string displayName = resource.GetString(resourceKey);
                return string.IsNullOrEmpty(displayName) ? resourceKey
                    : string.IsNullOrEmpty(keySuffix) ? displayName : displayName + keySuffix;
            }
        }

        /// <summary>
        /// Gets the description, stored in this attribute, reading from the resource using the cultureInfo defined by the language!
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>Description for the given language if found; the default Description or ressourceKey otherwise</returns>
        public string GetDescription(string language)
        {
            return resource.GetStringFromResourceForLanguage(resourceKey, language, Description, keySuffix);
        }

        /// <summary>
        /// Gets the key only.
        /// </summary>
        /// <returns>
        /// The value of the resource key.
        /// </returns>
        public string GetKeyOnly()
        {
            return resourceKey;
        }
    }
}
