using CounterBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CounterBot.Services
{
    public interface IUserStateService
    {
        void SetMode(long userId, BotMode mode);
        BotMode GetMode(long userId);
    }
}
