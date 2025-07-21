using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using DotNetEnv;
using CounterBot.Configuration;
using CounterBot.Controllers;

namespace BotCounter
{
    internal class Program
    {
        public static async Task Main()
        {
            //Загрузка .env из корня проекта
            string envPath = Path.Combine(Directory.GetCurrentDirectory(), "TelegramBotToken.env");

            //Диагностика(после проверки можно закомментировать)
            Console.WriteLine($"Ищем .env по пути: {envPath}");
            Console.WriteLine($"Файл существует: {File.Exists(envPath)}");

            if (!File.Exists(envPath))
            {
                //Альтернативный путь для отладки в IDE
                envPath = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ?? string.Empty,
                    "TelegramBotToken.env");

                Console.WriteLine($"Пробуем альтернативный путь: {envPath}");
                Console.WriteLine($"Файл существует: {File.Exists(envPath)}");
            }

            if (!File.Exists(envPath))
            {
                throw new FileNotFoundException($"Критическая ошибка: .env не найден ни в {Directory.GetCurrentDirectory()}, ни в корне проекта. " +
                                             "Убедитесь, что файл существует и имеет свойства 'Content/Copy always'");
            }
            Env.Load(envPath); //Явно указываем путь

            Console.OutputEncoding = Encoding.Unicode;

            // Объект, отвечающий за постоянный жизненый цикл приложения
            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) => ConfigureServices(services))//Задаем конфигурацию
                .UseConsoleLifetime()//Позволяет поддерживать приложение активным в консоли
                .Build();//Собираем

            Console.WriteLine("Сервис запущен");
            //Запускаем сервис
            await host.RunAsync();
            Console.WriteLine("Сервис остановлен");

            var downLoadFolder = new AppSettings();
        }

        /// <summary>
        /// Метод запуска постоянно активного сервиса
        /// и регистрация бота
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureServices(IServiceCollection services)
        {
            //Подключаем контроллеры сообщений и кнопок
            services.AddTransient<DefaultMessageController>();
            services.AddTransient<TextMessageController>();
            services.AddTransient<InlineKeyboardController>();
            
            AppSettings appSettings = BuildAppSettings();
            services.AddSingleton(appSettings);
            
            //Регистрируем объект TelegramBotClient с токеном подключения
            services.AddSingleton<ITelegramBotClient>
                (provider => new TelegramBotClient(appSettings.BotToken));            

            //Регистрируем постоянно активный сервис бота
            services.AddHostedService<Bot>();

            //Подключаем хранилище данных в памяти
            //_______

            //Метод инициализации конфигурации
            static AppSettings BuildAppSettings()
            {
                return new AppSettings()
                {                    
                    BotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? throw new InvalidOperationException("TELEGRAM_BOT_TOKEN не найден в TelegramBotToken.env"),
                };
            }
          
        }
    }
}
