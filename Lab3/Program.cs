using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab3
{
    class Program
    {
        static readonly string host = "http://prlab3-test.eu-central-1.elasticbeanstalk.com";
        static readonly Uri hostUri = new Uri(host);
        static readonly HttpClientHandler handler = new() {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        static readonly HttpClient httpClient = new HttpClient(handler) { BaseAddress = hostUri };
        static bool menuActive = true;
        static async Task Main(string[] args) {

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
                    Name = "Получить список пользователей",
                    Method = GetUserList
                },
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

        static JsonContent InputAuthData() {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();
            return JsonContent.Create(new {
                username,
                password
            });
        }

        static async ValueTask Register() {
            var postContent = InputAuthData();

            var response = await httpClient.PostAsync("/api/Auth/register", postContent);

            if (response.IsSuccessStatusCode) {
                Console.WriteLine("\nРегистрация прошла успешно!");
            } else {
                string message = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"\n{(int)response.StatusCode}: {message}");
            }
        }

        static async ValueTask Login() {
            var postContent = InputAuthData();

            var response = await httpClient.PostAsync("/api/Auth/login", postContent);
            if (response.IsSuccessStatusCode) {
                var cookies = response.Headers.GetValues("Set-Cookie").First();
                handler.CookieContainer.SetCookies(hostUri, cookies);
                Console.WriteLine("\nЛогин успешен! Срок действия полученного Cookie - 15 минут");
            } else {
                string message = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"\n{(int)response.StatusCode}: {message}");
            }
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

        static ValueTask Quit() {
            menuActive = false;
            return new ValueTask();
        }
    }

    class MenuAction
    {
        public string Name { get; set; }
        public Func<ValueTask> Method { get; set; }
    }

    class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public override string ToString() {
            return $"Id: {Id}, Username: {Username}";
        }
    }
}
