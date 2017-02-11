using System;

namespace CoPay
{
    public class customData
    {
        public String walletPrivKey { get; set; }
    }

    public class Copayer
    {
        public string name { get; set; }
        public string encryptedName { get; set; }
        public string xPubKey { get; set; }
        public string requestPubKey { get; set; }
        public string signature { get; set; }
    }
}
