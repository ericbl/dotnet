using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace Common.Network.Tests
{
    [TestClass()]
    public class EmailTests
    {
        private const string myEmail1 = "fillwithYourEmail@outlook.com";
        private const string myEmail2 = "fillwithYour2ndEmail@outlook.com";
        private const string myEmail3 = "fillwithYour2ndEmail@myDomain.com";

        [TestMethod()]
        public void CreateAndSendEMailOutlookTest()
        {
            NetworkCredential networkCredential = Windows.WebCredentialMgr.GetCredential(myEmail1);
            if (networkCredential != null)
            {
                Email email = new Email("smtp.live.com", true, networkCredential, myEmail2, "test email", "test email via outlook ssl", false, null);
                email.SendInTryCatch();
            }
        }

        [TestMethod()]
        public void CreateAndSendEMailLocalTest()
        {
            Email email = new Email(myEmail1, myEmail3, "test email", "test email from local server", null);
            email.SendInTryCatch();

        }
    }
}