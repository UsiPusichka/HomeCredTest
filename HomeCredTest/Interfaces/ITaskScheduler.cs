using HomeCredTest.Enums;
using System.Threading.Tasks;

namespace HomeCredTest.Interfaces
{
    public interface ITaskScheduler<TTask> where TTask : IHCTask
    {
        void Initialize(int numberOfTasks);
        TaskCondition Schedule(TTask task);
        Task Stop();
    }
}
