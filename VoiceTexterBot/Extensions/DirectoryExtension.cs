namespace VoiceTexterBot.Extensions
{
    public static class DirectoryExtension
    {
        /// <summary>
        /// Получаем путь до каталога с .sln файлом
        /// </summary>
        /// <returns></returns>    

        public static string GetSolutionRoot()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            if (directory == null || !directory.GetFiles("*.sln").Any())
            {
                throw new DirectoryNotFoundException(
                    $"Решение (.sln) не найдено! Поиск начинался с: {Directory.GetCurrentDirectory()}"
                );
            }
            return directory?.FullName;
        }
    }
}
