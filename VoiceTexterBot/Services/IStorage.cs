using VoiceTexterBot.Models;

namespace VoiceTexterBot.Services
{
    public interface IStorage
    {
        /// <summary>
        /// Получение сессия пользователя по идентификатору
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Session GetSession(long chatId);
    }
}
