using CounterBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CounterBot.Services
{
    public class UserStateService : IUserStateService
    {
        private readonly ConcurrentDictionary<long, BotMode> _userModes = new();

        public void SetMode(long userId, BotMode mode) => _userModes[userId] = mode;

        public BotMode GetMode(long userId) => _userModes.TryGetValue(userId, out var mode)
            ? mode: BotMode.CountChars; //Режим по умолчанию
    }
}
