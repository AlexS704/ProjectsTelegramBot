using CounterBot.Models;
using CounterBot.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CounterBot.Controllers
{
    public class InlineKeyboardController
    {
        private const string StartMenuText = "Выберите режим работы:";
        private const string ModeChangedText = "Режим изменен: {0}";

        private readonly ITelegramBotClient _telegramClient;
        private readonly IUserStateService _stateService;
        private readonly ILogger<InlineKeyboardController> _logger;

        // Словарь для соответствия callback data и режимов
        private static readonly Dictionary<string, BotMode> _callbackModes = new()
        {
            ["count_chars"] = BotMode.CountChars,
            ["sum_numbers"] = BotMode.SumNumbers
        };

        public InlineKeyboardController(
            ITelegramBotClient telegramBotClient,
            IUserStateService stateService,
            ILogger<InlineKeyboardController> logger
            )
        {
            _telegramClient = telegramBotClient;           
            _stateService = stateService;
            _logger = logger;
        }

        //Показывает главное меню при команде /start
        public async Task ShowStartMenu(long chatId, CancellationToken ct)
        {
            try
            {
                var buttons = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🔢 Сумма чисел", "sum_numbers"),
                        InlineKeyboardButton.WithCallbackData("📝 Подсчёт символов", "count_chars")
                    }
                });

                await _telegramClient.SendMessage(
                    chatId: chatId,
                    text: StartMenuText,
                    replyMarkup: buttons,
                    cancellationToken: ct
                    );
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при показе меню");
                throw;
            }         
        }
        
        public async Task Handle(CallbackQuery callbackQuery, CancellationToken ct)
        {
            if (callbackQuery?.Data == null || callbackQuery.From == null)
            {
                _logger.LogWarning("Получен некорректный CallbackQuery");
                return;
            }

            try
            {
                // Упрощенное получение режима через словарь
                if (!_callbackModes.TryGetValue(callbackQuery.Data, out var mode))
                {
                    _logger.LogWarning($"Неизвестный режим: {callbackQuery.Data}");
                    return;
                }

                //Сохраняем выбор пользователя
                _stateService.SetMode(callbackQuery.From.Id, mode);

                await _telegramClient.SendMessage(
                    chatId: callbackQuery.From.Id,
                    text: string.Format(ModeChangedText, GetModeDisplayName(mode)),
                    cancellationToken: ct
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки CallbackQuery");
                await HandleErrorAsync(callbackQuery?.From?.Id, ex, ct);
            }            
        }
        private async Task HandleErrorAsync(long? chatId, Exception ex, CancellationToken ct)
        {
            if (chatId.HasValue)
            {
                await _telegramClient.SendMessage(
                    chatId: chatId.Value,
                    text: "⚠️ Произошла ошибка при обработке запроса",
                    cancellationToken: ct);
            }
        }
        private string GetModeDisplayName(BotMode mode) => mode switch
        {
            BotMode.CountChars => "подсчёт символов",
            BotMode.SumNumbers => "сумма чисел",
            _ => "неизвестный режим"
        };
    }
}
