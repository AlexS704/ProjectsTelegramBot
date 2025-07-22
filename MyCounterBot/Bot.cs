
//Требования к боту:
//1. Бот должен иметь две функции: подсчёт количества символов в тексте и вычисление суммы чисел,
//которые вы ему отправляете (одним сообщением через пробел).
//2.То есть в ответ на условное сообщение «сова летит» он должен прислать что-то вроде
//«в вашем сообщении 10 символов». А в ответ на сообщение «2 3 15» должен прислать «сумма чисел: 20».
//3. Выбор одной из двух функций должен происходить на старте в «Главном меню».
//При старте (через /start) бот должен присылать клиенту ответное сообщение — меню с кнопками, из которого можно выбрать,
//какое действие пользователь хочет выполнить (по аналогии с тем, как мы выбирали язык в VoiceTexterBot).

using Microsoft.Extensions.Hosting; 
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using CounterBot.Controllers;
using CounterBot.Services;
using Microsoft.Extensions.Logging;

namespace BotCounter
{
    internal class Bot : BackgroundService
    {
        /// <summary>
        /// объект, отвечающий за отправку сообщений клиенту
        /// </summary>
        private ITelegramBotClient _telegramClient;

        private readonly IUserStateService _stateService;
        private readonly ILogger _logger;

        //Контроллеры различных видов сообщений
        private InlineKeyboardController _inlineKeyboardController;
        private TextMessageController _textMessageController;
        private DefaultMessageController _defaultMessageController;

        public Bot(
            ITelegramBotClient telegramClient,
            IUserStateService stateService,
            ILogger<Bot> logger,
            InlineKeyboardController inlineKeyboardController,
            TextMessageController textMessageController,
            DefaultMessageController defaultMessageController)
        {
            _telegramClient = telegramClient;
            _stateService = stateService;
            _logger = logger;
            _inlineKeyboardController = inlineKeyboardController;
            _textMessageController = textMessageController;
            _defaultMessageController = defaultMessageController;
        }     

        /// <summary>
        /// Метод активации бота и запуск в постоянно активном режиме
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions() { AllowedUpdates = Array.Empty<UpdateType>() },
                cancellationToken: stoppingToken);

            Console.WriteLine("Бот запущен");           
        }

        /// <summary>
        /// Метод обработки обновлений ботом
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, 
            CancellationToken ct)
        {
            //Обрабатываем нажатия на кнопки из Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
            try
            {
                switch (update.Type)
                {
                    case UpdateType.CallbackQuery:
                        await _inlineKeyboardController.Handle(update.CallbackQuery,
                    ct);
                        break;

                    case UpdateType.Message:
                        await HandleMessageAsync(update.Message, ct);
                        break;
                }
            }
            catch (Exception ex) 
            {
                await HandleErrorAsync(botClient, ex, ct);
            }
        }            
            private async Task HandleMessageAsync(Message message, CancellationToken ct)
            {
                if (message?.Text == "/start")
                {
                    await _inlineKeyboardController.ShowStartMenu(message.Chat.Id, ct);
                    return;
                }

                if (message?.Type == MessageType.Text)
                {
                    await _textMessageController.Handle(message, ct);
                }
                else
                {
                await _defaultMessageController.Handle(message, ct);
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
            CancellationToken ct)
        {
            _logger.LogError(exception, "Ошибка в боте");

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
            return Task.Delay(10_000, ct); // без блокировки потока            
        }
    }
}
