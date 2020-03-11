using Disbot.Classes;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Disbot.AI
{
    public static class NectecService
    {
        public static async Task<SSenseModel> CallSSenseService(string text)
        {
            var client = new RestClient($"https://api.aiforthai.in.th");
            client.AddDefaultHeader("Apikey", "aDsiiQrzcVTVS8TD7JvVn419fVJ6bcBf");
            var request = new RestRequest($"/ssense?text={text}");

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            var response = client.Execute(request);
            var content = response.Content;
            var ssense = Newtonsoft.Json.JsonConvert.DeserializeObject<SSenseModel>(content);
            return ssense;
        }
    }
}
