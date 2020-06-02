using HomeCredTest.Concrete;
using HomeCredTest.Models;
using System;

namespace HomeCredTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int maxNumberOfTasksAtSameTime = 0;
            int timeoutTypeOfWork = 1000;
            bool isNotAccept = true;
            Print.PrintEndTask = true;

            Console.WriteLine("Enter max number of tasks at same time: ");

            while (isNotAccept)
            {
                if (int.TryParse(Console.ReadLine(), out maxNumberOfTasksAtSameTime))
                {
                    isNotAccept = false;
                }
                else
                {
                    Console.WriteLine("Incorrect!");
                }
            }

            var taskSheduller = new HCTaskScheduler();
            taskSheduller.Initialize(maxNumberOfTasksAtSameTime);

            foreach (var task in HCTask.GetTestData(timeoutTypeOfWork))
                taskSheduller.Schedule(task);

            Console.ReadLine();
        }
    }
}
