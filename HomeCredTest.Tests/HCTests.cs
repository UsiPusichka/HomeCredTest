using HomeCredTest.Concrete;
using HomeCredTest.Enums;
using HomeCredTest.Interfaces;
using HomeCredTest.Models;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCredTest.Tests
{
    public class Tests
    {
        int timeoutTypeOfWork;
        [SetUp]
        public void Setup()
        {
            timeoutTypeOfWork = 100;
        }

        [Test]
        public void ReturnTaskConditionAndQueueTaskTest()
        {
            var taskSheduler = new HCTaskScheduler();
            taskSheduler.Initialize(4);

            foreach(var task in _GetTestData(timeoutTypeOfWork))
            {
                var taskCond = taskSheduler.Schedule(task);

                if (task.Id < 3 || task.Id == 5)
                    Assert.AreEqual(taskCond, TaskCondition.InProgress);
                if(task.Id == 4 || task.Id > 5)
                    Assert.AreEqual(taskCond, TaskCondition.InQueue);
            }
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void MaxNumberOfTasksAtSameTimeTest(int maxNumberOfTasksAtSameTime)
        {
            var taskSheduler = new HCTaskScheduler();
            taskSheduler.Initialize(maxNumberOfTasksAtSameTime);

            foreach (var task in _GetTestData(timeoutTypeOfWork))
            {
                taskSheduler.Schedule(task);
            }

            FieldInfo fieldInfo = typeof(HCTaskScheduler).GetField("_currenTasksList", BindingFlags.Instance | BindingFlags.NonPublic);
            var _currenTasksList = (ConcurrentDictionary<int, Task<IHCTask>>)fieldInfo.GetValue(taskSheduler);

            Assert.AreEqual(_currenTasksList.Count(), maxNumberOfTasksAtSameTime);
        }

        [Test]
        public void StopTest()
        {
            var taskSheduler = new HCTaskScheduler();
            taskSheduler.Initialize(4);

            foreach (var task in _GetTestData(timeoutTypeOfWork))
            {
                taskSheduler.Schedule(task);
            }
            var task1 = taskSheduler.Stop();

            task1.Wait();

            FieldInfo fieldInfo = typeof(HCTaskScheduler).GetField("_list", BindingFlags.Instance | BindingFlags.NonPublic);
            var _list = (ConcurrentDictionary<IHCTask, Task<IHCTask>>)fieldInfo.GetValue(taskSheduler);

            Assert.IsTrue(!_list.Any());
        }

        #region Private methods

        private static IEnumerable<IHCTask> _GetTestData(int timeout)
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
            yield return new HCTask(4, Priority.High, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(5, Priority.Normal, () =>
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
        #endregion
    }
}