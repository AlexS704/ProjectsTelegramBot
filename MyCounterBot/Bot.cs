using Microsoft.Extensions.Hosting; //библиотека для подключения постоянно активного сервиса 
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;


namespace VoiceTexterBot
{
    internal class Bot : BackgroundService
    {
        /// <summary>
        /// объект, отвечающий за отправку сообщений клиенту
        /// </summary>
        private ITelegramBotClient _telegramClient;

        public Bot(ITelegramBotClient telegramClient)
        {
            _telegramClient = telegramClient;
        }
        /// <summary>
        /// Метод запуска бота
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions() { AllowedUpdates = { } }, //Здесь выбираем, какие обновления хотим получать. В данном случае разрешены все)
                cancellationToken: stoppingToken);

            Console.WriteLine("Бот запущен");
            return Task.CompletedTask;
        }
        /// <summary>
        /// Метод обработки обновлений ботом
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, 
            CancellationToken cancellationToken)
        {
            //Обрабатываем нажатия на кнопки из Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
            if (update.Type == UpdateType.CallbackQuery)            
            {

                await _telegramClient.SendMessage
                    (update.CallbackQuery.From.Id,
                    $"Данный тип сообщений не поддерживается. Пожалуйста отправьте текст.",
                    cancellationToken: cancellationToken);
                return;
            }
            //Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
            if (update.Type == UpdateType.Message)
            {
                switch (update.Message!.Type)
                {
                    case MessageType.Text:
                    Console.WriteLine($"Получено сообщение: {update.Message.Text}\n" +
                        $"Длина: {update.Message.Text.Length} знаков");
                        await 
                    _telegramClient.SendMessage
                    (update.Message.From.Id,
                    text: $"Длина сообщения: {update.Message.Text.Length} знаков",
                    cancellationToken: cancellationToken);
                        return;

                    default:
                        await _telegramClient.SendMessage
                    (update.Message.From.Id,
                    $"Данный тип сообщений не поддерживается. Пожалуйста отправьте текст.",
                    cancellationToken: cancellationToken);
                        return;
                }                        
                  
            } 
        }   
        /// <summary>
        /// Метод обработки ошибок
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, 
            CancellationToken cancellationToken)
        {

            // Задаем сообщение об ошибке в зависимости от того, какая именно ошибка произошла
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
                       
            //Выводим в консоль информацию об ошибке
            Console.WriteLine(errorMessage);

            //Задержка перед повторным подключением
            Console.WriteLine("Ожидаем 10 секунд перед повторным подключением.");
            Thread.Sleep(10000);

            return Task.CompletedTask;
        }
    }
}
