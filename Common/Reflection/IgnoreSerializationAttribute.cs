using System;

namespace Common.Reflection
{
    /// <summary>
    /// Simple Attribute to mark a field or a property to be ignored during serialization
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class IgnoreSerializationAttribute : Attribute
    {
    }
}
