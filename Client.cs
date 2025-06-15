using RestSharp;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Zyntra.CS
{
    public class Client
    {
        private string? _token;
        private RestClient? _client;
        private bool _started;
        private const string api_version = "v1";
        private const string base_url = "https://zyntra.gg";

        public Client() { }

        private string ConstructBaseEndpoint() => $"api/{api_version}";
        private string ConstructEndpoint(string endpoint) => ConstructBaseEndpoint() + '/' + endpoint;
        private RestRequest AuthorizeRequest(RestRequest request) => request.AddHeader("Authorization", _token!);
        private bool InitializedProperly() => _token != null && _client != null && _started;

        private RestRequest ConstructSendMessageToAccessPointRequest(string accessPoint, string content) => AuthorizeRequest(new RestRequest(ConstructEndpoint($"channels/{accessPoint}/messages"), Method.Post).AddJsonBody(new { content }));
        public async Task<(bool status, string reason)> SendMessageToAccessPoint(string accessPoint, string content)
        {
            if (!InitializedProperly())
            {
                return (false, "not initialized properly");
            }

            RestResponse response = await _client.ExecuteAsync(ConstructSendMessageToAccessPointRequest(accessPoint, content));
            if (response == null)
            {
                return (false, "response was null");
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return (false, "status code was not OK");
            }

            return (true, "sent message");
        }

        private async Task<(bool status, string reason)> StartBot()
        {
            try
            {
                _client = new RestClient(base_url);
                _started = true;
                return (true, "succesfully started bot");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool status, string? reason)> Login(string Token)
        {
            _token = Token;
            return await StartBot();
        }
    }
}
