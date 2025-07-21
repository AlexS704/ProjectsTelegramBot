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

        public async Task Handle(Message message, CancellationToken ct)
        {
            if (message.Text is not { } text)
                return;

            try
            {
                string response = ProcessMessage(text);
                await SendResponse(message.Chat.Id, response, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await HandleError(message.Chat.Id, ex, ct);
            }           
        }

        private string ProcessMessage(string input)
        {
            if (IsNumberSequence(input))
            {
                double sum = CalculateSum(input);
                return $"Сумма чисел: {sum.ToString(CultureInfo.InvariantCulture)}";
            }
            return $"Количество символов: {input.Length}";
        }
        
        private bool IsNumberSequence(string input)
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.All(IsValidNumber);
        }

        private bool IsValidNumber(string part)
        {
            return double.TryParse(
                part,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out _
                );
        }

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

        private async Task SendResponse(long chatId, string text, CancellationToken ct)
        {
            Console.WriteLine($"Отправка в чат {chatId}: {text}");
            await Task.Delay(100, ct);//заглушка для имитации асинхронной отправки
        }

        private async Task HandleError(long chatId, Exception ex, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            await SendResponse(chatId, "Произошла ошибка при обработке сообщения", ct);
        }

//        Console.WriteLine($"Контроллер {GetType().Name} получил сообщение\n" + 
//                $"Длина Вашего сообщения: {message.Text.Length} знаков");
               
//            if(double.TryParse(message.Text, out double number))
//            {
//                await _telegramClient.SendMessage(message.Chat.Id,
//        text: $"Сумма всех Ваших чисел: {message.Text.Summ}",
//        cancellationToken: ct);
//    }
//            else
//            {
//                await _telegramClient.SendMessage(message.Chat.Id,
//        text: $"Длина Вашего сообщения: {message.Text.Length} знаков",
//        cancellationToken: ct);
//} 
            
    }
}
