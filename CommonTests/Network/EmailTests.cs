using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace Common.Network.Tests
{
    [TestClass()]
    public class EmailTests
    {
        [TestMethod()]
        public void CreateAndSendEMailOutlookTest()
        {
            NetworkCredential networkCredential = Windows.WebCredentialMgr.GetCredential("abacusdkh@outlook.com");
            if (networkCredential != null)
            {
                Email email = new Email("smtp.live.com", true, networkCredential, "eric.bleher@kloeckner.com", "test email", "test email via outlook ssl", false, null);
                email.SendInTryCatch();
            }
        }

        [TestMethod()]
        public void CreateAndSendEMailLocalTest()
        {
            Email email = new Email("abacusdkh@collab.com", "eric.bleher@kloeckner.com", "test email", "test email from local server", null);
            email.SendInTryCatch();

        }
    }
}