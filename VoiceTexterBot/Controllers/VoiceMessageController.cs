using Telegram.Bot;
using Telegram.Bot.Types;
using VoiceTexterBot.Configuration;
using VoiceTexterBot.Services;

namespace VoiceTexterBot.Controllers
{
    public class VoiceMessageController
    {        
        private readonly ITelegramBotClient _telegramClient;
        private readonly IFileHandler _audioFileHandler;
        private readonly IStorage _memoryStorage;

        public VoiceMessageController(ITelegramBotClient telegramClient, IFileHandler audioFileHandler, IStorage memoryStorage)
        {            
            _telegramClient = telegramClient;
            _audioFileHandler = audioFileHandler;
            _memoryStorage = memoryStorage;
        }

        /// <summary>
        /// Метод обработчик сообщений
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Handle(Message message, CancellationToken ct)
        {
            var fileId = message.Voice?.FileId;
            if (fileId == null)
                return;
            
            await
                _audioFileHandler.Download(fileId, ct);
                        
            //Получаем язык из сессии пользователя
            string userLanguageCode = _memoryStorage.GetSession(message.Chat.Id).LanguageCode;
            var result = _audioFileHandler.Process(userLanguageCode);

            //Запускаем обработку
            _audioFileHandler.Process(userLanguageCode);

            await
                _telegramClient.SendMessage(message.Chat.Id, result, cancellationToken: ct);

        }
    }
}
