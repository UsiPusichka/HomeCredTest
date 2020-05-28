using HomeCredTest.Interfaces;
using System;

namespace HomeCredTest.Concrete
{
    public class Print
    {
        public static object _locker = new object();
        public static bool PrintEndTask = false;
        public static void PrintStart(IHCTask task, DateTime date)
        {
            lock (_locker)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Task id={task.Id} with priority {task.Priority} started: {date.Millisecond}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        public static void PrintEnd(IHCTask task, DateTime date)
        {
            if(PrintEndTask)
                lock (_locker)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Task id={task.Id} with priority {task.Priority} finished: {date.Millisecond}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
        }
    }
}
