using HomeCredTest.Enums;
using HomeCredTest.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCredTest.Concrete
{
    public class HCTaskScheduler : ITaskScheduler<IHCTask>
    {
        #region Private fields and properties

        private static object _numberOfTasksAtSameTimeLocker = new object();
        private static object _isStopingLocker = new object();
        private const int _counterHighTasksLimit = 3;

        private bool _isInitialized = false;
        private bool _isStoping = false;
        private int _counterHighTasks;
        private int _numberOfTasksAtSameTime;
        private int _maxNumberOfTasksAtSameTime;
        private int _currentCountTasks;

        private ConcurrentDictionary<IHCTask,Task<IHCTask>> _list;
        private ConcurrentDictionary<int, Task<IHCTask>> _currenTasksList;

        private bool _IsStoping
        {
            get
            {
                return _isStoping;
            }
            set
            {
                lock (_isStopingLocker)
                {
                    _isStoping = value;
                }
            }
        }

        private int NumberOfTasksAtSameTime
        {
            get
            {
                return _numberOfTasksAtSameTime;
            }
            set
            {
                lock (_numberOfTasksAtSameTimeLocker)
                {
                    if (value < _numberOfTasksAtSameTime && value < _maxNumberOfTasksAtSameTime)
                    {
                        _numberOfTasksAtSameTime = value;
                        _fillQueue();
                    }
                    else
                        _numberOfTasksAtSameTime = value;
                }
            }
        }

        #endregion

        public void Initialize(int maxNumberOfTasksAtSameTime)
        {
            if (maxNumberOfTasksAtSameTime <= 0)
                throw new ArgumentException(nameof(maxNumberOfTasksAtSameTime));

            _maxNumberOfTasksAtSameTime = maxNumberOfTasksAtSameTime;
            _counterHighTasks = 0;
            _currentCountTasks = 0;

            _list = new ConcurrentDictionary<IHCTask, Task<IHCTask>>();
            _currenTasksList = new ConcurrentDictionary<int, Task<IHCTask>>();

            _isInitialized = true;
        }

        public TaskCondition Schedule(IHCTask task)
        {
            if (!_isInitialized)
                throw new Exception("TaskScheduler is not initialized. Plese call Initialize() method");
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (!_IsStoping)
            {
                task.TaskCondition = TaskCondition.InQueue;

                try
                {
                    _list.TryAdd(task, new Task<IHCTask>(() => _runTask(task)));
                    _currentCountTasks++;
                    if (NumberOfTasksAtSameTime <= _maxNumberOfTasksAtSameTime)
                    {
                        _fillQueue();
                    }

                    return task.TaskCondition;
                }
                catch
                {
                    task.TaskCondition = TaskCondition.NotDelivered;
                    return task.TaskCondition;
                }
            }
            else
            {
                task.TaskCondition = TaskCondition.NotDelivered;
                return task.TaskCondition;
            }

        }

        public Task Stop()
        {
            _IsStoping = true;

            var task = Task.WhenAll(_list.Select(x => x.Value).ToArray());
            task.ContinueWith(t => _IsStoping = false);

            return task;
        }
        #region Private methods

        private IHCTask _runTask(IHCTask task)
        {
            task.Run();
            return task;
        }

        private void _fillQueue()
        {
            foreach (var _task in _getTaskFromQueues())
            {
                _task.Value.Start();

                _task.Key.TaskCondition = TaskCondition.InProgress;

                Print.PrintStart(_task.Key, DateTime.Now);

                _task.Value.ContinueWith(t =>
                {
                    Print.PrintEnd(_task.Key, DateTime.Now);

                    t.Result.TaskCondition = TaskCondition.Finished;

                    if(_currenTasksList.TryRemove(_task.Key.Id, out var removedTask))
                    NumberOfTasksAtSameTime--;
                });

                _currenTasksList.TryAdd(_task.Key.Id, _task.Value);

                NumberOfTasksAtSameTime++;
            }
        }

        private IEnumerable<KeyValuePair<IHCTask, Task<IHCTask>>> _getTaskFromQueues()
        {
            if (NumberOfTasksAtSameTime == _maxNumberOfTasksAtSameTime)
                yield break;

            var task = _list.FirstOrDefault(x => x.Key.Priority == Priority.High);
            var taskKey = task.Key;

            if (taskKey != null && _counterHighTasks < _counterHighTasksLimit)
            {
                _list.TryRemove(taskKey, out var removed);
                _counterHighTasks++;
                yield return task;
            }
            else
                taskKey = null;

            if (taskKey == null)
            {
                task = _list.FirstOrDefault(x => x.Key.Priority == Priority.Normal);
                taskKey = task.Key;

                if (taskKey != null)
                {
                    _list.TryRemove(taskKey, out var removed);
                    _counterHighTasks = 0;
                    yield return task;
                }
            }
            if (taskKey == null)
            {
                task = _list.FirstOrDefault(x => x.Key.Priority == Priority.Low);
                taskKey = task.Key;

                if (taskKey != null)
                {
                    _list.TryRemove(taskKey, out var removed);
                    _counterHighTasks = 0;
                    yield return task;
                }
            }
            yield break;
        }

        #endregion
    }
}
