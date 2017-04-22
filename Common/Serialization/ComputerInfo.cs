using System.Runtime.Serialization;

namespace Common.Serialization
{
    /// <summary>
    /// Information about the computer.
    /// </summary>
    [DataContract]
    public class ComputerInfo
    {
        /// <summary>
        /// Gets or sets the name of the computer.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name="ComputerName", IsRequired = true)]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the windows version. (e.g. Windows 10 Entreprise)
        /// </summary>
        /// <value>
        /// The windows version.
        /// </value>
        [DataMember(Name = "WindowsVersion", IsRequired = false)]
        public string WindowsVersion { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the computer is running a x64 version of Windows.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Windows is X64; <c>false</c> if Windows 32 only.
        /// </value>
        [DataMember(Name = "IsX64", IsRequired = false)]
        public bool IsX64 { get; set; }
        /// <summary>
        /// Gets or sets the error messages.
        /// </summary>
        /// <value>
        /// The error messages.
        /// </value>
        [DataMember(Name = "ErrorMessages", IsRequired = false)]
        public string ErrorMessages { get; set; }
    }
}
