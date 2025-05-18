using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
