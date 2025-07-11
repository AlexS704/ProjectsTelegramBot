using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using VoiceTexterBot.Extensions;
using Vosk;

namespace VoiceTexterBot.Utilites
{
    public static class SpeechDetector
    {
        /// <summary>
        /// Метод обработчик
        /// </summary>
        /// <param name="audioPath">путь до файла</param>
        /// <param name="inputBitrate">загрузка битрейта</param>
        /// <param name="languageCode">загрузка языка</param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static string DetectSpeech(string audioPath, float inputBitrate, string languageCode)
        {
            try
            {               
                Console.WriteLine($"[DetectSpeech] Начало обработки, аудио: {audioPath}");
                Vosk.Vosk.SetLogLevel(0);
                var modelPath = Path.Combine(DirectoryExtension.GetSolutionRoot(),
                    "Speech-models",
                    $"vosk-model-small-{languageCode.ToLower()}");

                Console.WriteLine($"[DetectSpeech] Используется модель: {modelPath}");

                if (!Directory.Exists(modelPath))
                {
                    Console.WriteLine($"[ОШИБКА] Модель не найдена по пути: {modelPath}");
                    throw new DirectoryNotFoundException($"Модель {modelPath} не найдена");
                }

                using (var model = new Model(modelPath))
                {
                    Console.WriteLine($"[DetectSpeech] Модель успешно загружена");
                    return GetWords(model, audioPath, inputBitrate);
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ОШИБКА DetectSpeech] {ex}");
                throw;
            }        
        }

        /// <summary>
        /// Основной метод для распознования слов
        /// </summary>
        /// <param name="model"></param>
        /// <param name="audioPath"></param>
        /// <param name="inputBitrate"></param>
        /// <returns></returns>
        private static string GetWords(Model model, string audioPath, float inputBitrate)
        {

            Console.WriteLine($"[GetWords] Начало распознавания, битрейт: {inputBitrate}");

            //В конструктор для распознования передаем битрейт, а также используемую языковую модель
            
            var rec = new VoskRecognizer(model, inputBitrate);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);

            var textBuffer = new StringBuilder();
            int chunkCounter = 0;
            int emptyResults = 0;
                        
            try
            {
                using (var source = File.OpenRead(audioPath))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        chunkCounter++;
                        if (rec.AcceptWaveform(buffer, bytesRead))
                        {
                            var sentenceJson = rec.Result();
                            Console.WriteLine($"[GetWords] Промежуточный результат: {sentenceJson}");

                            var sentenceObj = JObject.Parse(sentenceJson);
                            var sentence = (string)sentenceObj["text"];

                            if (!string.IsNullOrWhiteSpace(sentence))
                            {
                                textBuffer.Append(StringExtension.UppercaseFirst(sentence) + ". ");
                                emptyResults = 0;
                            }
                            else
                            {
                                emptyResults++;
                                Console.WriteLine($"[GetWords] Получен пустой результат #{emptyResults}");
                            }
                        }

                        if (emptyResults >= 3)
                        {
                            Console.WriteLine("[GetWords] Слишком много пустых результатов, прерываем");
                            break;
                        }
                    }
                }

                // Получаем финальный результат
                var finalResult = rec.FinalResult();
                Console.WriteLine($"[GetWords] Финальный результат: {finalResult}");

                var finalSentenceObj = JObject.Parse(finalResult);
                var finalText = (string)finalSentenceObj["text"];

                if (!string.IsNullOrWhiteSpace(finalText))
                {
                    textBuffer.Append(finalText);
                }

                var fullText = textBuffer.ToString().Trim();
                Console.WriteLine($"[GetWords] Итоговый текст: {fullText}");

                return fullText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ОШИБКА GetWords] {ex}");
                throw;
            } 
        }
    }
}
