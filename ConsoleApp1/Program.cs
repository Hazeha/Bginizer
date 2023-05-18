using System;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

public class ImageData
{
    public string Url { get; set; }
}

public class ResponseData
{
    public ImageData[] Data { get; set; }
}
//{ "id":"cmpl-7HKbZ9UlnhnIqt4xTvrCJVRMfbc0C","object":"text_completion","created":1684365137,"model":"text-davinci-003","choices":[{ "text":"\n\nA dog is a four-legged, domesticated animal that is often kept as a pet and is known for its loyalty and companionship.","index":0,"logprobs":null,"finish_reason":"stop"}],"usage":{ "prompt_tokens":15,"completion_tokens":30,"total_tokens":45} }

public class ChatResponseData
{
    public string id { get; set; }
    public string @object { get; set; }
    public long created { get; set; }
    public string model { get; set; }
    public Choice[] choices { get; set; }
    public Usage usage { get; set; }
}

public class Choice
{
    public string text { get; set; }
    public int index { get; set; }
    public object logprobs { get; set; }
    public string finish_reason { get; set; }
}

public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}


public class Program
{
    static async Task Main()
    {
        
        string openaiApiKey = "xxxxxxx";
        string apiUrl = "https://api.openai.com/v1/images/generations";
        string apiUrl2 = "https://api.openai.com/v1/completions";

       // make cool welcome message
        Console.WriteLine("Welcome to the OpenAI Image Generator!");
        Console.WriteLine("This program will generate an image based on your input.");
        Console.WriteLine("It will then set that image as your desktop background.");
        Console.WriteLine("If you don't enter anything, it will use a default prompt.");
        Console.WriteLine("Hit enter to continue...");
        Console.ReadKey();
        Console.Clear();

        Console.WriteLine("What do you want as desktop background?");

        string prompt = Console.ReadLine();

        string defaultPrompt = "a photo of a happy corgi puppy sitting and facing forward, studio light, longshot";

        if (prompt == "")
        {
            prompt = defaultPrompt;
        }
        string requestBody = $@"{{
            ""prompt"": ""{prompt}"",
            ""n"": 1,
            ""size"": ""1024x1024""
        }}";

        var requestData = new
        {
            model = "text-davinci-003",
            prompt = "Explain to me in about 30 words what " + prompt + " is/means",
            max_tokens = 50,
            temperature = 0.2,
        };
        var requestDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);

        
        using (HttpClient client = new HttpClient())
        {
            Console.WriteLine("Generating image for " + prompt);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openaiApiKey}");
            HttpResponseMessage chatResponse = await client.PostAsync(apiUrl2, new StringContent(requestDataJson, Encoding.UTF8, "application/json"));
            var chatResponseContent = await chatResponse.Content.ReadAsStringAsync();
            ChatResponseData chatResponseData = JsonConvert.DeserializeObject<ChatResponseData>(chatResponseContent);
            Console.WriteLine(chatResponseData.choices[0].text);

            Console.WriteLine("Still generating image...");
            HttpResponseMessage response = await client.PostAsync(apiUrl, new StringContent(requestBody, Encoding.UTF8, "application/json"));
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Generated the image!");

        if (response.IsSuccessStatusCode)
        {
            WebClient webClient = new WebClient();
            ResponseData responseData = JsonConvert.DeserializeObject<ResponseData>(responseContent);
                Console.WriteLine("Saved your artpiece!");
            string imageUrl = responseData.Data[0].Url;
            byte[] data = webClient.DownloadData(imageUrl);

            await File.WriteAllBytesAsync("C:/Pictures/image.jpg", data);
       
            string file = System.IO.Directory.GetFiles(@"C:\Pictures\")[0];
                
            SystemParametersInfo(0x0014, 0, file, 0x0001 | 0x0002);
            Console.WriteLine("Successfully painted your desk with the buityfull " + prompt);

        }
        else
        {
            Console.WriteLine("API request failed with status code: " + response.StatusCode);
        }
        }
        Console.WriteLine("Hit enter to close me...");
        Console.ReadKey();
    }
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
}