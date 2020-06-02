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

        [Test, Order(1)]
        public void ReturnTaskConditionAndQueueTaskWithNormalPriorityTest()
        {
            var taskSheduler = new HCTaskScheduler();
            taskSheduler.Initialize(3);

            foreach (var task in _GetTestDataWithNormalPriority(timeoutTypeOfWork))
            {
                taskSheduler.Schedule(task);
            }

            var _currenTasksList = GetField<ConcurrentDictionary<int, Task<IHCTask>>>("_currenTasksList", taskSheduler);
            var ll = _currenTasksList.Keys.ToArray();

            Assert.AreEqual(ll[0], 1);
            Assert.AreEqual(ll[1], 2);
            Assert.AreEqual(ll[2], 3);

            Thread.Sleep(110);
            ll = _currenTasksList.Keys.ToArray();

            Assert.AreEqual(ll[0], 4);
            Assert.AreEqual(ll[1], 5);
            Assert.AreEqual(ll[2], 8);

            taskSheduler.Stop();
        }

        [Test, Order(2)]
        public void ReturnTaskConditionAndQueueTaskWithoutNormalPriorityTest()
        {
            var taskSheduler = new HCTaskScheduler();
            taskSheduler.Initialize(3);

            foreach (var task in _GetTestDataWithoutNormalPriority(timeoutTypeOfWork))
            {
                taskSheduler.Schedule(task);
            }

            var _currenTasksList = GetField<ConcurrentDictionary<int, Task<IHCTask>>>("_currenTasksList", taskSheduler);
            var ll = _currenTasksList.Keys.ToArray();

            Assert.AreEqual(ll[0], 1);
            Assert.AreEqual(ll[1], 2);
            Assert.AreEqual(ll[2], 3);

            Thread.Sleep(110);
            ll = _currenTasksList.Keys.ToArray();

            Assert.AreEqual(ll[0], 4);
            Assert.AreEqual(ll[1], 6);
            Assert.AreEqual(ll[2], 8);

            taskSheduler.Stop();
        }

        [Test, Order(3)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void MaxNumberOfTasksAtSameTimeTest(int maxNumberOfTasksAtSameTime)
        {
            var taskSheduler = new HCTaskScheduler();
            taskSheduler.Initialize(maxNumberOfTasksAtSameTime);

            foreach (var task in _GetTestDataWithNormalPriority(timeoutTypeOfWork))
            {
                taskSheduler.Schedule(task);
            }

            FieldInfo fieldInfo = typeof(HCTaskScheduler).GetField("_currenTasksList", BindingFlags.Instance | BindingFlags.NonPublic);
            var _currenTasksList = (ConcurrentDictionary<int, Task<IHCTask>>)fieldInfo.GetValue(taskSheduler);

            Assert.AreEqual(_currenTasksList.Count(), maxNumberOfTasksAtSameTime);
        }

        [Test, Order(4)]
        public void StopTest()
        {
            var taskSheduler = new HCTaskScheduler();
            taskSheduler.Initialize(4);

            foreach (var task in _GetTestDataWithNormalPriority(timeoutTypeOfWork))
            {
                taskSheduler.Schedule(task);
            }
            var task1 = taskSheduler.Stop();

            task1.Wait();

            var _queue1 = GetField<Queue<(IHCTask, Task<IHCTask>)>>("_highQueue", taskSheduler);
            var _queue2 = GetField<Queue<(IHCTask, Task<IHCTask>)>>("_normalQueue", taskSheduler);
            var _queue3 = GetField<Queue<(IHCTask, Task<IHCTask>)>>("_lowQueue", taskSheduler);

            Assert.IsTrue(!_queue1.Any());
            Assert.IsTrue(!_queue2.Any());
            Assert.IsTrue(!_queue3.Any());
        }

        #region Private methods

        private T GetField<T>(string fieldName, HCTaskScheduler taskSheduler)
        {
            FieldInfo fieldInfo = typeof(HCTaskScheduler).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)fieldInfo.GetValue(taskSheduler);
        }

        private static IEnumerable<IHCTask> _GetTestDataWithNormalPriority(int timeout)
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
            yield return new HCTask(7, Priority.Low, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(8, Priority.High, () =>
            {
                Thread.Sleep(timeout);
            });
        }

        private static IEnumerable<IHCTask> _GetTestDataWithoutNormalPriority(int timeout)
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
            yield return new HCTask(5, Priority.Low, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(6, Priority.High, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(7, Priority.Low, () =>
            {
                Thread.Sleep(timeout);
            });
            yield return new HCTask(8, Priority.High, () =>
            {
                Thread.Sleep(timeout);
            });
        }
        #endregion
    }
}