using VoiceTexterBot.Models;

namespace VoiceTexterBot.Services
{
    public interface IStorage
    {
        /// <summary>
        /// Получение сессии пользователя по идентификатору
        /// </summary>
        /// <param name="chatId">уникальный идентификатор</param>
        /// <returns></returns>
        Session GetSession(long chatId);
    }
}
