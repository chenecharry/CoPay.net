using System;

namespace CoPay.CreateWallet
{
    public class Response
    {
        public Guid walletId { get; set; }
    }
}

namespace CoPay.JoinWallet
{
    public class Response : CoPay.CreateWallet.Request
    {
        public Copayer [] copayers { get; set; }
    }
}
