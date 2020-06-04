using Newtonsoft.Json;
using PoETS.Data.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.API {
    public class HttpClient {
        private readonly System.Net.Http.HttpClient Client;

        public HttpClient() {
            Client = new System.Net.Http.HttpClient();
        }

        public async Task<string> Query(string url) {
            List<string> urls = new List<string>() { url };
            Dictionary<string, List<string>> objUrls = new Dictionary<string, List<string>>() { { "urls", urls } };
            var bodyContent = JsonConvert.SerializeObject(objUrls);

            var response = await Client.PostAsync(ConfigManager.GetConfig().PoETSServer, new StringContent(bodyContent, Encoding.UTF8, "application/json"));

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<Dictionary<string, string>> Query(List<string> urls) {
            var bodyContent = JsonConvert.SerializeObject(new Dictionary<string, List<string>>() { { "urls", urls } });

            var response = await Client.PostAsync(ConfigManager.GetConfig().PoETSServer, new StringContent(bodyContent, Encoding.UTF8, "application/json"));

            var raw = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(raw);
        }
    }
}
