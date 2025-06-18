using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace Zyntra.CS
{
    public class Client
    {
        private string? _token;
        private RestClient? _client;
        private bool _started;
        private const string api_version = "v1";
        private const string base_url = "https://zyntra.gg";
        public string BaseURL
        {
            get
            {
                return base_url + "/" + ConstructBaseEndpoint();
            }
        }

        public Client() { }

        private string ConstructBaseEndpoint() => $"api/{api_version}";
        private string ConstructEndpoint(string endpoint) => ConstructBaseEndpoint() + '/' + endpoint;
        private RestRequest AuthorizeRequest(RestRequest request) => request.AddHeader("Authorization", _token!).AddHeader("User-Agent", UserAgent.Random);
        private bool InitializedProperly() => _token != null && _client != null && _started;

        private RestRequest ConstructGetMessageRequest(long accessPointId, long messageId) => AuthorizeRequest(new RestRequest(ConstructEndpoint($"channels/{accessPointId}/messages/{messageId}"), Method.Get));
        private RestRequest ConstructSendMessageRequest(long accessPointId, string content) => AuthorizeRequest(new RestRequest(ConstructEndpoint($"channels/{accessPointId}/messages"), Method.Post).AddJsonBody(new { content }));
        public async Task<(bool status, string reason, PartialMessage? msg)> SendMessage(AccessPoint accessPoint, string content)
        {
            if (!InitializedProperly())
            {
                return (false, "not initialized properly", null);
            }

            RestResponse response = await _client!.ExecuteAsync(ConstructSendMessageRequest(accessPoint.ID, content));
            if (response == null)
            {
                return (false, "response was null", null);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return (false, "status code was not OK", null);
            }

            if (response.ContentType == null || response.ContentType != "application/json")
            {
                return (false, "response content type was not application/json", null);
            }

            if (response.Content == null)
            {
                return (false, "response content was null", null);
            }

            JObject message_response;

            try
            {
                message_response = JObject.Parse(response.Content);
            }
            catch
            {
                return (false, "response content deserialization failed", null);
            }

            if (message_response["success"]?.Type != JTokenType.Boolean || !(message_response["success"]?.Value<bool>() ?? false))
            {
                return (false, "message creation failed", null);
            }

            if (message_response["messageId"]?.Type != JTokenType.Integer)
            {
                return (false, "unexpected response from server", null);
            }

            PartialMessage msg = new PartialMessage { ID = message_response["messageId"]!.Value<long>(), AP = accessPoint };
            return (true, "sent message", msg);
        }

        private async Task<(bool status, string reason)> DeleteMessage(PartialMessage message)
        {
            return (false, "TBD"); // well szymekk didnt respond ❤️
        }

        public async Task<(bool status, string reason, Message? msg)> GetMessage(PartialMessage message)
        {
            if (!InitializedProperly())
            {
                return (false, "not initialized properly", null);
            }

            RestResponse response = await _client!.ExecuteAsync(ConstructGetMessageRequest(message.AP.ID, message.ID));
            if (response == null)
            {
                return (false, "response was null", null);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine(response.StatusCode);
                return (false, "status code was not OK", null);
            }

            if (response.ContentType == null || response.ContentType != "application/json")
            {
                return (false, "response content type was not application/json", null);
            }

            if (response.Content == null)
            {
                return (false, "response content was null", null);
            }

            JObject message_response;

            try
            {
                message_response = JObject.Parse(response.Content);
            }
            catch
            {
                return (false, "response content deserialization failed", null);
            }

            if (message_response["messageContent"]?.Type != JTokenType.String)
            {
                return (false, "unexpected response from server", null);
            }

            if (message_response["bucketId"]?.Type != JTokenType.Integer)
            {
                return (false, "unexpected response from server", null);
            }

            if (message_response["sender"]?.Type != JTokenType.Object)
            {
                return (false, "unexpected response from server", null);
            }

            if (message_response["inCache"]?.Type != JTokenType.Boolean)
            {
                return (false, "unexpected response from server", null);
            }

            if (message_response["sender"]?.Type != JTokenType.Object)
            {
                return (false, "unexpected response from server", null);
            }

            JToken sender_token = message_response["sender"]!;
            if (sender_token["id"]?.Type != JTokenType.String)
            {
                return (false, "unexpected response from server", null);
            }

            if (sender_token["username"]?.Type != JTokenType.String)
            {
                return (false, "unexpected response from server", null);
            }

            User sender = new User()
            {
                ID = sender_token["id"].Value<string>(),
                Name = sender_token["username"].Value<string>()
            };

            Message msg = new Message()
            {
                ID = message.ID,
                AP = message.AP,
                Sender = sender,
                BucketID = message_response["bucketId"].Value<long>(),
                InCache = message_response["inCache"].Value<bool>(),
                Content = message_response["messageContent"].Value<string>()
            };

            return (true, "succesfully retrieved message", msg); // well szymekk didnt respond ❤️
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
