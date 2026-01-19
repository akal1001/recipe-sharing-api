using Newtonsoft.Json;
using System.Text;

namespace postplateapi.OpenAiManagers
{
    public class OpenAiManager
    {
    
        public string id { get; set; }
        public string @object { get; set; }
        public long created { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
        public List<Choice> choices { get; set; }
        //private string apiKey;
        private HttpClient httpClient;

        private string AwsAccessKeyId;
        private string AwsSecretAccessKey;
        private string BucketName;



        //api key
        private string OpenAiApiKey;
        //image generatie
        private const string DALLERequestUrl = "https://api.openai.com/v1/images/generations";
        //Chat completions
        private const string apiUrl = "https://api.openai.com/v1/chat/completions";
        private const string apiUrlImageGen = "https://api.openai.com/v1/images/generations";

        public OpenAiManager()  
        {
            AwsAccessKeyId = "xxxx";
            AwsSecretAccessKey = "xxxxx";
            BucketName = "sm-image-bucket";
           
            OpenAiApiKey = "xxxxx";

        }


        public async Task<OpenAIResponse> GetChatCompletionAsync(string prompt)
        {
            OpenAIResponse openAIResponse;
            try
            {


                httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {OpenAiApiKey}");


                var requestData = new
                {
                    model = "gpt-3.5-turbo",
                    //model = "gpt-4-turbo",
                    format = "json",
                    messages = new[]
                    {
                  new { role = "user", content = prompt }
               }
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);

                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error: {response.StatusCode} - {responseContent}");
                }


                openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);


                return openAIResponse;

            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return new OpenAIResponse();
            }

        }
        public async Task<string> GenerateImageAsync(string prompt)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {OpenAiApiKey}");

                var requestData = new
                {
                    prompt = prompt,
                    n = 1, // number of images
                    size = "512x512", // or "1024x1024", etc.
                    response_format = "url" // can also be "b64_json"
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrlImageGen, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Image generation failed: {response.StatusCode} - {responseContent}");
                }

                // Optional: define a small response model
                dynamic result = JsonConvert.DeserializeObject(responseContent);
                string imageUrl = result.data[0].url;

                return imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image generation error: {ex.Message}");
                return null;
            }
        
        }


    }

    public class OpenAIResponse
    {
        public string id { get; set; }
        public string @object { get; set; }
        public long created { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
        public Choice[] choices { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class Choice
    {
        public Message message { get; set; }
        public string finish_reason { get; set; }
        public int index { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}

