using FFMpegCore;
using VoiceTexterBot.Extensions;

namespace VoiceTexterBot.Utilites
{
    public static class AudioConverter
    {
        public static void TryConvert(string inputFile, string outputFile)
        {
            //Задаем путь, где лежит программа-конвертер
            GlobalFFOptions.Configure(options => options.BinaryFolder =
            Path.Combine(DirectoryExtension.GetSolutionRoot(), "ffmpeg-win64", "bin"));

            //вызываем Ffmpeg, передав требуемые аргументы

            FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, true, options => options
                .WithFastStart())
                .ProcessSynchronously();


        }
    }
}
