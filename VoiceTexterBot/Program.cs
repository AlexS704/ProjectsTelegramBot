using DotNetEnv;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Telegram.Bot;
using VoiceTexterBot.Configuration;
using VoiceTexterBot.Controllers;
using VoiceTexterBot.Services;

namespace VoiceTexterBot //имя тестового бота VoiceATextBot
{
    internal class Program
    {
        
        public static async Task Main()
        {
                        
            //Загрузка .env из корня проекта
            string envPath = Path.Combine(Directory.GetCurrentDirectory(), "TelegramBotToken.env");

            // Диагностика (можно удалить после проверки)
            //Console.WriteLine($"Ищем .env по пути: {envPath}");
            //Console.WriteLine($"Файл существует: {File.Exists(envPath)}");

            if (!File.Exists(envPath))
            {
                // Альтернативный путь для отладки в IDE
                envPath = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ?? string.Empty,
                    "TelegramBotToken.env");

                Console.WriteLine($"Пробуем альтернативный путь: {envPath}");
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
        }
        static void ConfigureServices(IServiceCollection services)
        {
            AppSettings appSettings = BuildAppSettings();
            services.AddSingleton(appSettings);
           
            //Подключаем контроллеры сообщений и кнопок
            services.AddTransient<DefaultMessageController>();
            services.AddTransient<VoiceMessageController>();
            services.AddTransient<TextMessageController>();
            services.AddTransient<InlineKeyboardController>();
            
            //Регистрируем объект TelegramBotClient с токеном подключения
            services.AddSingleton<ITelegramBotClient>
                (provider => new TelegramBotClient(appSettings.BotToken));
            
            //Регистрируем постоянно активный сервис бота
            services.AddHostedService<Bot>();

            //Подключаем хранилище данных в памяти
            services.AddSingleton<IStorage, MemoryStorage>();
            //Подключаем обработчик аудио файлов
            services.AddSingleton<IFileHandler, AudioFileHandler>();

            //Метод инициализации конфигурации
            static AppSettings BuildAppSettings()
            {              
                return new AppSettings()
                {
                    DownloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads"), //динамическое определение пути
                    BotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ??
                    throw new InvalidOperationException("TELEGRAM_BOT_TOKEN не найден в TelegramBotToken.env"),
                    AudioFileName = "audio",
                    InputAudioFormat = "ogg",
                };
            }
        }
    }
}
