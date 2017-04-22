using System;
using System.ComponentModel;

namespace Common.Reflection
{
    /// <summary>
    /// A special Description attribute meant for path
    /// </summary>
    /// <seealso cref="DescriptionAttribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PathDescriptionAttribute : DescriptionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">The description text.</param>
        public PathDescriptionAttribute(string description) : base(description)
        {
        }
    }
}
