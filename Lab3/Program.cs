using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab3
{
    class Program
    {
        static async Task Main(string[] args) {
            var host = "http://prlab3-test.eu-central-1.elasticbeanstalk.com";
            var httpClient = new HttpClient { BaseAddress = new Uri(host) };

            Console.WriteLine("POST:");
            var postContent = JsonContent.Create(new {
                username = "Kek",
                password = "cheburek"
            });
            await httpClient.PostAsync("/api/Auth/register", postContent);
            var result = await httpClient.PostAsync("/api/Auth/login", postContent);
            var cookies = result.Headers.GetValues("Set-Cookie");
            httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
            Console.WriteLine(result);
            Console.WriteLine();

            Console.WriteLine("GET:");
            result = await httpClient.GetAsync("/api/Users");
            Console.WriteLine(result);
            Console.WriteLine($"Content: {await result.Content.ReadAsStringAsync()}");
            Console.WriteLine();

            Console.WriteLine("OPTIONS:");
            result = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Options, "/api/Users"));
            Console.WriteLine(result);
            Console.WriteLine();

            Console.WriteLine("HEAD:");
            result = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/api/Users"));
            Console.WriteLine(result);
        }
    }
}
