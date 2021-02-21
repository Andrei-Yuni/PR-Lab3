using MihaZupan;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab3
{
    class Program
    {
        static readonly string host = "http://prlab3-test.eu-central-1.elasticbeanstalk.com";
        static readonly Uri hostUri = new Uri(host);
        static readonly HttpClientHandler handler = new() {
            AutomaticDecompression = DecompressionMethods.All,
            Proxy = new HttpToSocks5Proxy("127.0.0.1", 9050)
        };
        static readonly HttpClient httpClient = new HttpClient(handler) { BaseAddress = hostUri };
        static bool menuActive = true;
        static async Task Main() {
            var options = new[] {
                new MenuAction {
                    Name = "Регистрация",
                    Method = Register
                },
                new MenuAction {
                    Name = "Логин",
                    Method = Login
                },
                new MenuAction {
                    Name = "GET /api/Users",
                    Method = GetUserList
                },
                new MenuAction {
                    Name = "HEAD /api/Users",
                    Method = HeadUserList
                },
                new MenuAction {
                    Name = "OPTIONS /api/Users"
                }
                new MenuAction {
                    Name = "Выход",
                    Method = Quit
                }
            };
            while (menuActive) {
                Console.WriteLine("Меню: ");
                for (int i = 0; i < options.Length; i++) {
                    Console.WriteLine($"{i + 1}. {options[i].Name}");
                }
                Console.Write("\nВыберите опцию: ");
                uint input;
                while (!uint.TryParse(Console.ReadLine(), out input) || input < 1 || input > options.Length) {
                    Console.Write("Попробуйте еще раз: ");
                }
                Console.Clear();
                await options[input - 1].Method();
                Console.WriteLine("\nНажмите любую клавишу...");
                Console.ReadKey(true);
                Console.Clear();
            }
        }

        static HttpContent InputAuthData() {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();
            var content = new StringContent(JsonSerializer.Serialize(new {
                username,
                password
            }));
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
            return content;
        }

        static async ValueTask Register() {
            var postContent = InputAuthData();

            var response = await httpClient.PostAsync("/api/Auth/register", postContent);

            string message = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n{(int)response.StatusCode}: {message}");
        }

        static async ValueTask Login() {
            var postContent = InputAuthData();

            var response = await httpClient.PostAsync("/api/Auth/login", postContent);
            if (response.Headers.TryGetValues("set-cookie", out var cookies))
                handler.CookieContainer.SetCookies(hostUri, cookies.First());
            string message = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n{(int)response.StatusCode}: {message}");
        }

        static async ValueTask GetUserList() {
            var response = await httpClient.GetAsync("/api/Users");
            if (response.IsSuccessStatusCode) {
                var users = await response.Content.ReadFromJsonAsync<User[]>(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                foreach (var user in users) {
                    Console.WriteLine(user);
                }
            } else if (response.StatusCode == HttpStatusCode.Unauthorized) {
                Console.WriteLine("\nВы не авторизованы или срок действия cookie истёк");
            }
        }

        static async ValueTask HeadUserList() {
            var request = new HttpRequestMessage(HttpMethod.Head, "/api/Users");
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode) {
                if (response.Content.Headers.TryGetValues("content-length", out var contentLength)) {
                    Console.WriteLine($"Content-Length: {contentLength.First()}");
                }
            } else if (response.StatusCode == HttpStatusCode.Unauthorized) {
                Console.WriteLine("\nВы не авторизованы или срок действия cookie истёк");
            }
        }

        static ValueTask Quit() {
            menuActive = false;
            return new ValueTask();
        }
    }
}
