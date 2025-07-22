using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using DotNetEnv;
using CounterBot.Configuration;
using CounterBot.Controllers;
using CounterBot.Services;
using Microsoft.Extensions.Logging;

namespace BotCounter
{
    internal class Program
    {
        public static async Task Main()
        {
            try
            {
                Console.OutputEncoding = Encoding.Unicode;
                LoadEnvironment();
                ValidateToken();

                var host = new HostBuilder()
                    .ConfigureServices(ConfigureServices)
                    .ConfigureLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Information);
                    })
                    .UseConsoleLifetime()
                    .Build();

                Console.WriteLine("Сервис запущен");
                await host.RunAsync();
                Console.WriteLine("Сервис остановлен");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Environment.Exit(1);
            }
        }

            private static void LoadEnvironment()
        {
            const string envFile = "TelegramBotToken.env";

            // Поиск .env в разных локациях
            var locations = new[]
            {
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?
                    .Parent?.Parent?.FullName ?? string.Empty)
            };

            foreach (var location in locations)
            {
                var path = Path.Combine(location, envFile);
                if (File.Exists(path))
                {
                    Env.Load(path);
                    return;
                }
            }

            throw new FileNotFoundException($"Файл {envFile} не найден. Проверьте пути: {string.Join(", ", locations)}");
        }

        private static void ValidateToken()
        {
            var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            if (string.IsNullOrWhiteSpace(token) || token.Length < 30)
            {
                throw new InvalidOperationException(
                    "Некорректный TELEGRAM_BOT_TOKEN. Проверьте .env файл");
            }
        }

        static void ConfigureServices(IServiceCollection services)
        {
            // Конфигурация
            services.AddSingleton(new AppSettings
            {
                BotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")!
            });

            // Сервисы
            services.AddSingleton<IUserStateService, UserStateService>();

            // Контроллеры
            services.AddTransient<DefaultMessageController>();
            services.AddTransient<TextMessageController>();
            services.AddTransient<InlineKeyboardController>();

            // Telegram Client
            services.AddSingleton<ITelegramBotClient>(provider =>
                new TelegramBotClient(provider.GetRequiredService<AppSettings>().BotToken));

            // Hosted Service
            services.AddHostedService<Bot>();
        }
    }
}
