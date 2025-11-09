using System;
using System.Text;

namespace EntityFrameworkCoreLearning.Utils
{
    public enum TaskStatus
    {
        Running,
        Success,
        Failure
    }

    public static class ConsoleOutputHelper
    {
        private const string ResetColor = "\x1b[0m";
        private const string GreenColor = "\x1b[32m";
        private const string YellowColor = "\x1b[33m";
        private const string RedColor = "\x1b[31m";
        private const string CyanColor = "\x1b[36m";
        private const string Bold = "\x1b[1m";

        public static void WriteTaskTitle(string title)
        {
            Console.WriteLine($"\n{new string('-', 60)}");
            Console.Write($"{Bold}{CyanColor}[ЗАДАЧА] {title}{ResetColor}");
        }

        public static void WriteTaskStatus(TaskStatus status)
        {
            string statusText;
            string color;

            switch (status)
            {
                case TaskStatus.Success:
                    statusText = "ВЫПОЛНЕНА УСПЕШНО";
                    color = GreenColor;
                    break;
                case TaskStatus.Failure:
                    statusText = "ЗАВЕРШЕНА С ОШИБКОЙ";
                    color = RedColor;
                    break;
                case TaskStatus.Running:
                    statusText = "ВЫПОЛНЕНИЕ...";
                    color = YellowColor;
                    break;
                default:
                    statusText = "НЕИЗВЕСТНО";
                    color = ResetColor;
                    break;
            }

            Console.WriteLine($"{Bold}{color} => {statusText} {ResetColor}");
        }

        public static void WriteTaskResult(string result)
        {
            Console.WriteLine($"{Bold}--- РЕЗУЛЬТАТ ---{ResetColor}");
            Console.WriteLine(result);
            Console.WriteLine(new string('-', 60));
        }
    }
}
