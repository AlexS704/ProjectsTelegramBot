﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VoiceTexterBot.Services;

namespace VoiceTexterBot.Controllers
{
    public class InlineKeyboardController
    {
        private readonly ITelegramBotClient _telegramClient;
        private readonly IStorage _memoryStorage;
        
        public InlineKeyboardController (ITelegramBotClient telegramClient, IStorage memoryStorage)
        {
            _telegramClient = telegramClient;
            _memoryStorage = memoryStorage;
        }

        public async Task Handle(CallbackQuery? callbackQuery, CancellationToken ct)
        {
           if (callbackQuery?.Data == null)
                return;

            //Обновление пользовательской сессии новыми данными
            _memoryStorage.GetSession(callbackQuery.From.Id).LanguageCode = callbackQuery.Data;

            //Генерим информационное сообщение
            string languageText = callbackQuery.Data
                switch
            {
                "ru" => " Русский",
                "en" => " Aнглийский",
                "fr" => " Французский",
                "cn" => " Китайский",
                _ => String.Empty,
            };

            //Отправляем в ответ уведомление о выборе
            await
                _telegramClient.SendMessage(callbackQuery.From.Id, $"<b>Язык аудио - {languageText}.{Environment.NewLine}</b>"
                + $"{Environment.NewLine}Можно поменять в главном меню.", cancellationToken: ct, parseMode:ParseMode.Html);
        }
    }
}
