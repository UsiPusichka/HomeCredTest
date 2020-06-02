using HomeCredTest.Enums;
using HomeCredTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HomeCredTest.Models
{
    public class HCTask : IHCTask
    {
        public int Id { get; private set; }
        public Priority Priority { get; set; }
        public TaskCondition TaskCondition { get; set; }

        private Action _action;

        public HCTask(int id, Priority priority, Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Id = id;
            Priority = priority;
            _action = action;
        }

        public void Run() =>
            _action();

        public static IEnumerable<IHCTask> GetTestData(int timeout)
        {
            yield return new HCTask(1, Priority.High, () =>
            {
                //type of work
                Thread.Sleep(timeout);
            });
            yield return new HCTask(2, Priority.High, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(3, Priority.High, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(4, Priority.Normal, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(5, Priority.High, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(6, Priority.Low, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(7, Priority.Normal, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(8, Priority.High, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(9, Priority.Low, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(10, Priority.Low, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(11, Priority.High, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(11, Priority.Normal, () =>
            {
                Thread.Sleep(timeout);
            });
        }
    }
}
