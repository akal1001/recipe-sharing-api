using System.Text.Json;

namespace postplateapi.UnsplashManager
{

    public class UnsplashService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _clientId;
        private readonly string _apiUrlAws;

        public UnsplashService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiUrl = $"{config["UnsplashAPI:ApiUrl"]}/photos";
            _clientId = config["UnsplashAPI:Key"];
            _apiUrlAws = $"{config["UnsplashAPI:ApiUrlAws"]}/photos";
        }

        public async Task<string> GetPhotosAsync(string query)
        {
            var url = $"{_apiUrl}?query={query}&client_id={_clientId}";
            var responseString = await _httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            if (root.TryGetProperty("results", out JsonElement results) && results.GetArrayLength() > 0)
            {
                var firstPhoto = results[0];
                if (firstPhoto.TryGetProperty("urls", out JsonElement urls) &&
                    urls.TryGetProperty("small", out JsonElement smallUrl))
                {
                    return smallUrl.GetString();
                }
            }

            return null; // or string.Empty or throw exception as needed
        }

    }


}
