namespace Common.Network
{
    using System.Net.NetworkInformation;

    /// <summary>
    /// Utilities class for network operations
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Pings the given host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address.</param>
        /// <returns>True if ping is successful</returns>
        public static bool PingHost(string hostNameOrAddress)
        {
            if (!string.IsNullOrEmpty(hostNameOrAddress))
            {
                Ping p = new Ping();
                PingReply reply = p.Send(hostNameOrAddress);
                return reply.Status == IPStatus.Success;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified string is a proper IP v4 address.
        /// </summary>
        /// <param name="strIP">The string ip.</param>
        /// <returns>
        ///   <c>true</c> if the specified string is a proper IP v4 address; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsProperIPv4(string strIP)
        {
            //  Split string by ".", check that array length is 4
            string[] arrOctets = strIP.Split('.');
            if (arrOctets.Length != 4)
                return false;

            //Check each substring checking that parses to byte
            byte obyte = 0;
            foreach (string strOctet in arrOctets)
                if (!byte.TryParse(strOctet, out obyte))
                    return false;

            return true;
        }
    }
}
