using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CounterBot.Controllers
{
    public class DefaultMessageController
    {
        private const string UnsupportedFormatMessage = "⚠️ Я работаю только с текстовыми сообщениями.\n" +
                                                      "Отправьте текст или выберите режим в /start";

        private readonly ITelegramBotClient _telegramClient;
        private readonly ILogger<DefaultMessageController> _logger;

        public DefaultMessageController(
            ITelegramBotClient telegramBotClient,
            ILogger<DefaultMessageController> logger)
        {
            _telegramClient = telegramBotClient;
            _logger = logger;
        }

        public async Task Handle(Message message, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Получено сообщение типа {MessageType}", message.Type);

                await _telegramClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: UnsupportedFormatMessage,
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки сообщения типа {MessageType}", message?.Type);

                if (message?.Chat.Id != null)
                {
                    await TrySendErrorMessage(message.Chat.Id, ct);
                }
            }
        }

        private async Task TrySendErrorMessage(long chatId, CancellationToken ct)
        {
            try
            {
                await _telegramClient.SendMessage(
                    chatId: chatId,
                    text: "🚫 Произошла ошибка при обработке сообщения",
                    cancellationToken: ct);
            }
            catch
            {
                // Гарантированно не выбрасываем исключение
                _logger.LogCritical("Не удалось отправить сообщение об ошибке");
            }
        }
    }
}
