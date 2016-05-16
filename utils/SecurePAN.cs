using System;
using System.Security;
using System.Runtime.InteropServices;
using SafeNetWS.login;

namespace SafeNetWS.utils
{
    public class SecurePAN
    {
        private SecureString secureStr;

        /// <summary>
        /// Creating a SecureString is not as simple as a regular string object. 
        /// A SecureString is created one character at a time. 
        /// The class is designed this way to encourage the data to be captured directly as the user types it into an application. 
        /// However some applications will need to copy an existing string into a SecureString, at which point adding a character at a time is sufficient.
        /// </summary>
        /// <param name="pan">Plaintext pan</param>
        public SecurePAN(string pan)
        {
            if (String.IsNullOrEmpty(pan)) throw new ArgumentNullException("Empty value!");

            this.secureStr = Util.ConvertToSecureString(pan);
        }


        /// <summary>
        /// Reading a SecureString is more complicated. 
        /// There is no simple ToString method, which is also intended to keep the data secure. 
        /// To read the data C# developers must access the data in memory directly. Luckily the .NET Framework makes it fairly simple:
        /// </summary>
        /// <returns></returns>
        public string GetPAN()
        {
            return Util.ConvertToUnsecureString(this.secureStr);
        }
    }

}