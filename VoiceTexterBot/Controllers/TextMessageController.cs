﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace VoiceTexterBot.Controllers
{
    public class TextMessageController
    {
        private readonly ITelegramBotClient _telegramClient;

        public TextMessageController (ITelegramBotClient telegramClient)
        {
            _telegramClient = telegramClient;
        }

        public async Task Handle(Message message, CancellationToken ct)
        {
            switch (message.Text)
            {
                case "/start":
                    
                    //Объект, представляющий кнопки
                    var buttons = new List<InlineKeyboardButton[]>();
                    buttons.Add(new[]
                    {
                        InlineKeyboardButton.WithCallbackData($"Русский", $"ru"),
                        InlineKeyboardButton.WithCallbackData($"English", $"en"),
                        InlineKeyboardButton.WithCallbackData($"Français", $"fr"),
                        InlineKeyboardButton.WithCallbackData($"中国人", $"cn")
                    });

                    //передаем кнопки вместе с сообщением (параметр ReplyMarkup)
                    await
                        _telegramClient.SendMessage(message.Chat.Id, $"<b> Наш бот превращает аудио в текст.</b> {Environment.NewLine}"
                        + $"{Environment.NewLine}Можно записать сообщение и переслать другу, если лень печатать. {Environment.NewLine}",
                        cancellationToken: ct, parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(buttons));
                    break;
                    
                default:
                    await
                        _telegramClient.SendMessage(message.Chat.Id, "Отправьте аудио для превращения в текст.",
                        cancellationToken: ct);
                    break;
            }
        }
    }
}
