using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoPay.Tests
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void Should_Get_CoPay_Hash()
        {
            String actual = CoPay.Utils.getCopayerHash("lucas", "", "");
        }

        [TestMethod]
        public void Should_Sign_Message()
        {
            //const String keyAsHex = "09458c090a69a38368975fb68115df2f4b0ab7d1bc463fc60c67aa1730641d6c";

            var actual = Utils.signMessage("hola", "09458c090a69a38368975fb68115df2f4b0ab7d1bc463fc60c67aa1730641d6c");
            //should.exist(sig);
            var expected = "1fab6b1331d1be00878833ea620c72bb657ba95a3fa8cc653b3829db359216b0e178384e38f87f92f5a9dee763f480d84fcde86e291b91f4fa699482209d0590e2";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Should_Hash_Message()
        {
            String actual = Utils.hashMessage("hola");
            Assert.AreEqual("4102b8a140ec642feaa1c645345f714bc7132d4fd2f7f6202db8db305a96172f", actual);
            
            //res.toString('hex').should.equal('4102b8a140ec642feaa1c645345f714bc7132d4fd2f7f6202db8db305a96172f');
        }
    }
}
