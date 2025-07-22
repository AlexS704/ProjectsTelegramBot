using Telegram.Bot;
using Telegram.Bot.Types;
using System.Globalization;
using CounterBot.Models;
using CounterBot.Services;
using Microsoft.Extensions.Logging;


namespace CounterBot.Controllers
{
    public class TextMessageController
    {
        private readonly ITelegramBotClient _telegramClient;
        private readonly IUserStateService _stateService;
        private readonly ILogger<TextMessageController> _logger;

        public TextMessageController(
            ITelegramBotClient telegramBotClient,
            IUserStateService stateService,
            ILogger<TextMessageController> logger
            )
        {
            _telegramClient = telegramBotClient;
            _logger = logger;
            _stateService = stateService;
        }

        /// <summary>
        /// Метод обработки сообщений
        /// </summary>
        /// <param name="message">принимает сообщение</param>
        /// <param name="ct">токен отмены</param>
        /// <returns></returns>
        public async Task Handle(Message message, CancellationToken ct)
        {
            //проверяем, что сообщение содержит текст
            if (message.Text is not { } text)
                return; // выходим, если нет текста          
                    
            try
            {
                var mode = _stateService.GetMode(message.Chat.Id);
                string response = mode switch
                {
                    BotMode.CountChars => $"📝Символов: {text.Length}",
                    BotMode.SumNumbers => TryCalculateSum(text) is double sum
                    ? $"🔢 Сумма: {sum}"
                    : "❌ Отправьте только числа через пробел!",
                    _ => "Выберите режим в /start"
                };

                await _telegramClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: response,
                    cancellationToken: ct);               
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await HandleError(message.Chat.Id, ex, ct);
            }           
        }

        /// <summary>
        /// Метод сложения чисел из сообщения
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private double? TryCalculateSum(string input)
        {
            try
            {
                return input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => double.Parse(part, NumberStyles.Any, CultureInfo.InvariantCulture))
                    .Sum();
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private async Task HandleError(long chatId, Exception ex, CancellationToken ct)
        {
            _logger.LogError(ex, "Ошибка обработки сообщения");
            var errorText = ex switch
            {
                FormatException => "⚠️ Некорректный формат чисел",
                _ => "⚠️ Ошибка обработки сообщения"
            };

            await _telegramClient.SendMessage(
                chatId: chatId,
                text: errorText,
                cancellationToken: ct
                );

            Console.WriteLine($"Ошибка: {ex.Message}");
        }            
    }
}
