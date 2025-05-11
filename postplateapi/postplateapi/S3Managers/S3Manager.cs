
using System.Text.Json;
using System.Text;

namespace postplateapi.S3Managers
{
    public class S3Manager
    {
        private const string apiUrl = "http://qqqqq.somee.com/api/s3/UploadFromUrl";
        private const string apiKey = "UnJ4QeZfY1dudOC0Tt2wMJmN8/2w1piw+boeqxc0sfey7ttrZtisq/ukAiX2lfTj";
       
        public async Task<(string FileUrl, string Key)> UploadImageFromUrlAsync(string imageUrl)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("apiKey", apiKey); // Set API key from header

                var jsonBody = JsonSerializer.Serialize(imageUrl);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var _response = JsonSerializer.Deserialize<UploadResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (_response != null && _response.Success)
                {
                    return (_response.Data.FileUrl, _response.Data._Key);
                }

                throw new Exception($"Upload failed: {_response?.Message ?? response.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image upload error: {ex.Message}");
                return (null, null);
            }
        }

        //original method that wroks 
        public async Task<string> UploadImageFromUrlAsync_1(string imageUrl)
        {
            try
            {
                
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("apiKey", apiKey); // Not Authorization since your API uses FromHeader

                var jsonBody = JsonSerializer.Serialize(imageUrl);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

          
                var _response = JsonSerializer.Deserialize<UploadResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true  
                });

              

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Image upload failed: {response.StatusCode} - {responseContent}");
                }

                return responseContent; // return actual API response (which includes the S3 URL etc.)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image generation error: {ex.Message}");
                return null;
            }
        }

    }
    public class UploadResponse
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public UploadData Data { get; set; }
    }
    public class UploadData
    {
        public string FileUrl { get; set; }
        public string _Key { get; set; }
        public string Status { get; set; }
    }
}
