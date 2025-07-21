using Telegram.Bot;
using Telegram.Bot.Types;
using System.Globalization;


namespace CounterBot.Controllers
{
    public class TextMessageController
    {
        private readonly ITelegramBotClient _telegramClient;

        public TextMessageController(ITelegramBotClient telegramBotClient)
        {
            _telegramClient = telegramBotClient;
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
                string response = ProcessMessage(text);
                await _telegramClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: response,
                    cancellationToken: ct
                );
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await HandleError(message.Chat.Id, ex, ct);
            }           
        }

        /// <summary>
        /// Метод обработки сообщения по его типу
        /// </summary>
        /// <param name="input">тип сообщения</param>
        /// <returns></returns>
        private string ProcessMessage(string input)
        {
            //если числа
            if (IsNumberSequence(input))
            {
                double sum = CalculateSum(input);
                return $"Сумма чисел: {sum.ToString(CultureInfo.InvariantCulture)}";
            }
            //если текст
            return $"Количество символов: {input.Length}";
        }
        
        /// <summary>
        /// Проверка строку на одни числа
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool IsNumberSequence(string input)
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.All(IsValidNumber);
        }

        /// <summary>
        /// Проверка на валидность числа
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private bool IsValidNumber(string part)
        {
            return double.TryParse(
                part,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out _
                );
        }

        /// <summary>
        /// Метод сложения чисел из сообщения
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private double CalculateSum(string input)
        {
            return input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => double.Parse(
                    part,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture
                    ))
                .Sum();
        }

        private async Task SendResponse(
            long chatId, 
            string text, 
            CancellationToken ct)
        {
            Console.WriteLine($"Отправка в чат {chatId}: {text}");
            await Task.Delay(100, ct);//заглушка для имитации асинхронной отправки
        }

        private async Task HandleError(long chatId, Exception ex, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            await _telegramClient.SendMessage(
                chatId: chatId,
                text: "Произошла ошибка при обработке сообщения",
                cancellationToken: ct
                );
        }            
    }
}
