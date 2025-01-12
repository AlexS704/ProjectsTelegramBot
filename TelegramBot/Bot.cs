using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace TelegramBot
{
    /// <summary>
    /// Основной класс
    /// </summary>
    internal class Bot
    {
        /// <summary>
        /// объект, отвечающий за отправку сообщений клиенту
        /// </summary>
        private IBotClient _telegramClient;

        public Bot(IBotClient telegramClient)
        {
            _telegramClient = telegramClient;
        }

        /// <summary>
        /// Метод, регистриующий обработчеки-хандлеры
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _telegramClient.OnMessage += HandleMessage;
            _telegramClient.OnMessage += HandleButtonClick;

            Console.WriteLine("Bot started");
        }

        /// <summary>
        /// Обработчик входящих текстовых сообщений
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task HandleMessage(object sender, MessageEventArgs e)
        {
            //Бот получил входящее сообщение пользователя
            var messageText = e.Message.Text;

            //Бот отправляет ответ
            _telegramClient.SendTextMessage(e.ChatId, "Ответ на сообщение пользователя");
        }        

        /// <summary>
        /// Обработчик нажатий на кнопку
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task HandleButtonClick(object sender, MessageEventArgs e)
        {

        }
    }
}
