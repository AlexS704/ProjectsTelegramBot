using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            //Подключаем хранилище
            services.AddSingleton<IStorage, MemoryStorage>();

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
                    BotToken = "7832162276:AAHD7O4hbxCkuNnMMHjxYKY7LzBXE9Yq_zo",
                    AudioFileName = "audio",
                    InputAudioFormat = "ogg",
                };
            }
        }
    }
}
