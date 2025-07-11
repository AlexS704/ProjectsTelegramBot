using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VoiceTexterBot.Controllers;


namespace VoiceTexterBot
{
    internal class Bot : BackgroundService
    {
        // Клиент к Telegram Bot API
        private ITelegramBotClient _telegramClient;

        // Контроллеры различных видов
        private InlineKeyboardController _inlineKeyboardController;
        private TextMessageController _textMessageController;
        private VoiceMessageController _voiceMessageController;
        private DefaultMessageController _defaultMessageController;

        public Bot(ITelegramBotClient telegramClient,
            InlineKeyboardController inlineKeyboardController,
            TextMessageController textMessageController,
            VoiceMessageController voiceMessageController,
            DefaultMessageController defaultMessageController)
        {
            _telegramClient = telegramClient;
            _inlineKeyboardController = inlineKeyboardController;
            _textMessageController = textMessageController;
            _voiceMessageController = voiceMessageController;
            _defaultMessageController = defaultMessageController;
        }
        /// <summary>
        /// Запускает бота в постоянно активном режиме
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
        /// Метод(асинхронный) обработки обычных событий
        /// </summary>
        /// <param name="botClient">интерфес для работы с Bot Api</param>
        /// <param name="update">идентификатор обновления</param>
        /// <param name="cancellationToken">отменяет зависшую или долгую операцию</param>
        /// <returns></returns>
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, 
            CancellationToken cancellationToken)
        {
            //Обрабатываем нажатия на кнопки из Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
            if (update.Type == UpdateType.CallbackQuery)            
            {
                await
                    _inlineKeyboardController.Handle(update.CallbackQuery, cancellationToken);
                return;                
            }

            //Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
            if (update.Type == UpdateType.Message)
            {
               switch (update.Message!.Type)
                {
                    case MessageType.Voice:
                        await
                            _voiceMessageController.Handle(update.Message, cancellationToken);
                        return;

                    case MessageType.Text:
                        await
                            _textMessageController.Handle(update.Message, cancellationToken);
                        return;
 
                    default:
                        await
                            _defaultMessageController.Handle(update.Message, cancellationToken);
                        return;
                }
            } 
        }

        /// <summary>
        /// Метод(асинхронный) обработки ошибок
        /// </summary>
        /// <param name="botClient">интерфес для работы с Bot Api</param>
        /// <param name="exception">исключение</param>
        /// <param name="cancellationToken">отменяет зависшую или долгую операцию</param>
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
