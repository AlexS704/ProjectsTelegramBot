using FFMpegCore;
using FFMpegCore.Enums;
using System.Security.Cryptography.X509Certificates;
using VoiceTexterBot.Extensions;

namespace VoiceTexterBot.Utilites
{
    public static class AudioConverter
    {
        public static void TryConvert(string inputFile, string outputFile)
        {
            // Логирование начала конвертации
            Console.WriteLine($"[Конвертация] Начало обработки файла: {inputFile}");

            try
            {
                //Задаем путь, где лежит программа-конвертер
                GlobalFFOptions.Configure(options => options.BinaryFolder =
                Path.Combine(DirectoryExtension.GetSolutionRoot(), "ffmpeg-win64", "bin"));

                if (!File.Exists(inputFile))
                    throw new FileNotFoundException("Input file not found");

                //Вызываем Ffmpeg, передав требуемые аргументы
                FFMpegArguments
                    .FromFileInput(inputFile)
                    .OutputToFile(outputFile, true, options => options
                        .WithCustomArgument("-ar 96000") //явно задаем частоту 96кГц
                        .WithCustomArgument("-ac 1") //принудительно делаем моно
                        .WithCustomArgument("-acodec pcm_s16le") //16 битный кодек PCM
                        .WithCustomArgument("-af \"highpass=f=300,lowpass=f=3000\"") //фильтр частот
                        .WithFastStart())
                    .ProcessSynchronously();

                // Логирование успеха
                Console.WriteLine($"[Конвертация] Успешно сохранено в: {outputFile}");
                // Дополнительная проверка файла
                LogFileInfo(outputFile);
            }
            catch (Exception ex)
            {
                // Логирование ошибок
                Console.WriteLine($"[ОШИБКА] Конвертация не удалась: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw; // Пробрасываем исключение дальше  
            }            
        }  

        // Метод для логирования информации о файле
        private static void LogFileInfo(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                Console.WriteLine($"[Проверка] Размер файла: {fileInfo.Length} байт");

                if (fileInfo.Length == 0)
                {
                    Console.WriteLine("[ВНИМАНИЕ] Файл пуст!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ОШИБКА] Не удалось проверить файл: {ex.Message}");
            }
        }
    }
}
