using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CoPay
{
    public class Client
    {
        private readonly string BWS_INSTANCE_URL = "https://bws.bitpay.com/bws/api";
        private Cred m_Cred;

        public Client(String baseUrl = "https://bws.bitpay.com/bws/api")
        {
            BWS_INSTANCE_URL = baseUrl;
            m_Cred = new Cred()
            {
                network = "testnet",
                requestPrivKey = "tprv8dxkXXLevuHXR3tLvBkaDLyCnQxsQQVafnDMEQNds8r8tjSPfNTGD5ShtpP8QeTdtCoWGmrMC5gs9j7ap8ATdSsAD2KCv87BGdzPWwmdJt2",
                xPrivKey = "cNaQCDwmmh4dS9LzCgVtyy1e1xjCJ21GUDHe9K98nzb689JvinGV"
            };
        }

        public Client(String copayerName, String baseUrl = "https://bws.bitpay.com/bws/api") : this(baseUrl)
        {
            m_Cred.copayerId = copayerName;
        }

        Cred cred => m_Cred;

        public async Task<string> createWallet(String walletName, String copayerName, Int16 m, Int16 n, opts opts)
        {
            cred.copayerId = copayerName;

            CreateWallet.Request request = new CreateWallet.Request()
            {
                m = m,
                n = n,
                name = walletName,
                pubKey = "02fcba7ecf41bc7e1be4ee122d9d22e3333671eb0a3a87b5cdf099d59874e1940f",
                network = "testnet"
            };

            String url = BWS_INSTANCE_URL + "/v2/wallets/";
            String reqSignature = Utils.signRequest("GET", url, request, cred.xPrivKey);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-identity", cred.copayerId);
            client.DefaultRequestHeaders.Add("x-signature", reqSignature);

            String json = JsonConvert.SerializeObject(request);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpResponseMessage responseMessage = await client.PostAsync(url, requestContent))
            {
                if (responseMessage.IsSuccessStatusCode)
                {
                    String responseContent = await responseMessage.Content.ReadAsStringAsync();

                    CreateWallet.Response response = JsonConvert.DeserializeObject<CreateWallet.Response>(responseContent);
                    String share = buildSecret(response.walletId, cred.xPrivKey, "testnet");
                    return share;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public async Task doJoinWallet(Guid walletId, String walletPrivKey, String xPubKey, 
            String requestPubKey, String copayerName, opts opts) // callback?
        {
            opts Opts = opts??new opts();
            Opts.customData.walletPrivKey = walletPrivKey;

            String json = JsonConvert.SerializeObject(Opts.customData);

            var encCustomData = Utils.encryptMessage(json, m_Cred.personalEncryptingKey);
            var encCopayerName = Utils.encryptMessage(copayerName, m_Cred.sharedEncryptingKey);

            JoinWalletArgs args = new JoinWalletArgs()
            {
                walletId = walletId,
                name = encCopayerName,
                xPubKey = xPubKey,
                requestPubKey = requestPubKey,
                customData = encCustomData
            };

            var hash = Utils.getCopayerHash(args.name, args.xPubKey, args.requestPubKey);
            args.copayerSignature = Utils.signMessage(hash, walletPrivKey);

            var url = String.Format("/v2/wallets/{0}/copayers", walletId);
            String jsonArgs = JsonConvert.SerializeObject(args);
            String message = String.Format("{0}|{1}|{2}", "post", url, json);
            NBitcoin.BitcoinSecret s = new BitcoinSecret(cred.xPrivKey);
            String reqSignature = s.PrivateKey.SignMessage(message);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-identity", cred.copayerId);
            client.DefaultRequestHeaders.Add("x-signature", reqSignature);

            String jsonSerialized = JsonConvert.SerializeObject(args);
            StringContent requestContent = new StringContent(jsonSerialized, Encoding.UTF8, "application/json");

            using (HttpResponseMessage responseMessage = await client.PostAsync(url, requestContent))
            {
                if (responseMessage.IsSuccessStatusCode)
                {
                    String responseContent = await responseMessage.Content.ReadAsStringAsync();
                    JoinWallet.Response response = JsonConvert.DeserializeObject<JoinWallet.Response>(responseContent);

                    // API.prototype._processWallet
                    string encryptingKey = m_Cred.sharedEncryptingKey;
                    var decryptedName = Utils.decryptMessage(response.name, encryptingKey);
                    response.name = decryptedName;
                    foreach (var copayer in response.copayers)
                    {
                        var name = Utils.decryptMessage(copayer.name, encryptingKey);
                        if (name != copayer.name)
                        {
                            copayer.encryptedName = copayer.name;
                        }
                        copayer.name = name;

                        // Do something with access. Work under progress on access.
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

        }

        private static string buildSecret(Guid walletId, String walletPrivKey, string network)
        {
            string widHx = walletId.ToString("N");

            string widBase58 = Encoders.Base58.EncodeData(Encoders.Hex.DecodeData(widHx));

            return widBase58 + walletPrivKey + "L";
        }
    }
}
