using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

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
        /// <summary>
        /// Метод запуска постоянно активного сервиса
        /// и регистрация бота
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureServices(IServiceCollection services)
        {
            //Регистрируем объект TelegramBotClient с токеном подключения
            services.AddSingleton<ITelegramBotClient>
                (provider => new TelegramBotClient("7522450219:AAEdZMalwLrcWm3ei9SsQasEHuGBA96ywTc"));
            //Регистрируем постоянно активный сервис бота
            services.AddHostedService<Bot>();
        }
    }
}
