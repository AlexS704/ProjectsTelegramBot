namespace VoiceTexterBot.Services
{
    public interface IFileHandler
    {
        /// <summary>
        /// Метод загрузки
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task Download(string fileId, CancellationToken ct);
        string Process(string param);
    }
}
