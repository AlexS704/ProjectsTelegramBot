using VoiceTexterBot.Configuration;
using Telegram.Bot;
using VoiceTexterBot.Utilites;
using System.IO;
using System.Threading;


namespace VoiceTexterBot.Services
{
    public class AudioFileHandler : IFileHandler
    {
        private readonly AppSettings _appSettings;
        private readonly ITelegramBotClient _telegramBotClient;

        public AudioFileHandler(ITelegramBotClient telegramBotClient, AppSettings appSettings)
        {
            _telegramBotClient = telegramBotClient;
            _appSettings = appSettings;
        }

        public async Task Download(string fileId, CancellationToken ct)
        {
            //Генерируем полный путь файла из конфигурации
            string inputAudioFilePath = Path.Combine(_appSettings.DownloadsFolder,
                $"{_appSettings.AudioFileName}.{_appSettings.InputAudioFormat}");

            if (!Directory.Exists(_appSettings.DownloadsFolder))
                Directory.CreateDirectory(_appSettings.DownloadsFolder);

            try
            {
                using (FileStream destinationStream = File.Create(inputAudioFilePath))
                {
                    //Загружаем информацию о файле
                    var file = await
                        _telegramBotClient.GetFile(fileId, ct);
                    if (file.FilePath == null)
                        return;

                    //Скачивание файла
                    await
                        _telegramBotClient.DownloadFile(file.FilePath,
                        destinationStream, ct);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download error: {ex.Message}");
                throw;
            }     
        }

        public string Process(string languageCode)
        {
            string inputAudioPath = Path.Combine(_appSettings.DownloadsFolder,
                $"{_appSettings.AudioFileName}.{_appSettings.InputAudioFormat}");
            string outputAudioPath = Path.Combine(_appSettings.DownloadsFolder,
                $"{_appSettings.AudioFileName}.{_appSettings.OutputAudioFormat}");

            Console.WriteLine("Начинаем конвертацию...");AudioConverter
                .TryConvert(inputAudioPath, outputAudioPath);
            Console.WriteLine("Файл конвертирован");

            Console.WriteLine("Начинаем распознование...");
            var speechText = SpeechDetector.DetectSpeech(outputAudioPath, _appSettings.InputAudioBitrate, languageCode);
            Console.WriteLine("Файл распознан.");

            return speechText;
        }
    }
}
