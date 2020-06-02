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

        private int _countNormalTask;

        private Queue<(IHCTask, Task<IHCTask>)> _highQueue;
        private Queue<(IHCTask, Task<IHCTask>)> _normalQueue;
        private Queue<(IHCTask, Task<IHCTask>)> _lowQueue;

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
            _countNormalTask = 0;

            _highQueue = new Queue<(IHCTask, Task<IHCTask>)>();
            _normalQueue = new Queue<(IHCTask, Task<IHCTask>)>();
            _lowQueue = new Queue<(IHCTask, Task<IHCTask>)>();

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
                    switch (task.Priority)
                    {
                        case Priority.High:
                            _highQueue.Enqueue((task, new Task<IHCTask>(() => _runTask(task))));
                            break;
                        case Priority.Normal:
                            _normalQueue.Enqueue((task, new Task<IHCTask>(() => _runTask(task))));
                            _countNormalTask++;
                            break;
                        case Priority.Low:
                            _lowQueue.Enqueue((task, new Task<IHCTask>(() => _runTask(task))));
                            break;
                    }

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
            var _list = new List<Task<IHCTask>>();

            _list.AddRange(_highQueue.Select(x => x.Item2));
            _list.AddRange(_normalQueue.Select(x => x.Item2));
            _list.AddRange(_lowQueue.Select(x => x.Item2));

            var task = Task.WhenAll(_list.ToArray());
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
            foreach (var _task in _getTaskFromQueues1())
            {
                _task.Item2.Start();

                _task.Item1.TaskCondition = TaskCondition.InProgress;

                Print.PrintStart(_task.Item1, DateTime.Now);

                _task.Item2.ContinueWith(t =>
                {
                    Print.PrintEnd(_task.Item1, DateTime.Now);

                    t.Result.TaskCondition = TaskCondition.Finished;

                    if (_currenTasksList.TryRemove(_task.Item1.Id, out var removedTask))
                        NumberOfTasksAtSameTime--;
                });

                _currenTasksList.TryAdd(_task.Item1.Id, _task.Item2);

                NumberOfTasksAtSameTime++;
            }
        }

        private IEnumerable<(IHCTask, Task<IHCTask>)> _getTaskFromQueues1()
        {
            if (NumberOfTasksAtSameTime == _maxNumberOfTasksAtSameTime)
                yield break;

            (IHCTask, Task<IHCTask>) task;
            IHCTask taskKey = null;

            if (_highQueue.TryPeek(out task))
                taskKey = task.Item1;

            if (taskKey != null && (_counterHighTasks < _counterHighTasksLimit || _countNormalTask == 0))
            {
                _highQueue.Dequeue();
                _counterHighTasks++;
                yield return task;
            }
            else
                taskKey = null;

            if (taskKey == null)
            {
                if (_normalQueue.TryPeek(out task))
                    taskKey = task.Item1;

                if (taskKey != null)
                {
                    _normalQueue.Dequeue();
                    _counterHighTasks = 0;
                    _countNormalTask--;
                    yield return task;
                }
            }
            if (taskKey == null)
            {
                if (_lowQueue.TryPeek(out task))
                    taskKey = task.Item1;

                if (taskKey != null)
                {
                    _lowQueue.Dequeue();
                    _counterHighTasks = 0;
                    yield return task;
                }
            }
            yield break;
        }

        #endregion
    }
}
