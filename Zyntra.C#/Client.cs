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

        public Client() { }

        private string ConstructBaseEndpoint() => $"api/{api_version}";
        private string ConstructEndpoint(string endpoint) => ConstructBaseEndpoint() + '/' + endpoint;
        private RestRequest AuthorizeRequest(RestRequest request) => request.AddHeader("Authorization", _token!); // calling this with a null token shouldn't happen. initializedproperly should be checked
        private bool InitializedProperly() => _token != null && _client != null && _started;

        private RestRequest ConstructSendMessageRequest(long accessPoint, string content) => AuthorizeRequest(new RestRequest(ConstructEndpoint($"channels/{accessPoint}/messages"), Method.Post).AddJsonBody(new { content }));
        public async Task<(bool status, string reason, Message? msg)> SendMessage(long accessPoint, string content)
        {
            if (!InitializedProperly())
            {
                return (false, "not initialized properly", null);
            }

            RestResponse response = await _client!.ExecuteAsync(ConstructSendMessageRequest(accessPoint, content));
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

            if (message_response["messageId"] == null || message_response["messageId"]?.Type != JTokenType.Integer)
            {
                return (false, "unexpected response from server", null);
            }

            if (message_response["date"] == null || message_response["date"]?.Type != JTokenType.String)
            {
                return (false, "unexpected response from server", null);
            }

            long messageID = message_response["messageId"]!.Value<long>();
            string date = message_response["date"]!.Value<string>() ?? "";

            DateTime dateParsed = DateTime.MinValue;

            try
            {
                dateParsed = DateTime.Parse(date, null, DateTimeStyles.AdjustToUniversal);
            }
            catch (FormatException) { }

            Message msg = new Message { DateTime = dateParsed, ID = messageID };
            return (true, "sent message", msg);
        }

        private async Task<(bool status, string reason)> DeleteMessage(long accessPoint, long messageId)
        {
            return (false, "TBD"); // well szymekk didnt respond ❤️
        }

        private async Task<(bool status, string reason)> GetMessage(long accessPoint, long messageId)
        {
            return (false, "TBD"); // well szymekk didnt respond ❤️
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
