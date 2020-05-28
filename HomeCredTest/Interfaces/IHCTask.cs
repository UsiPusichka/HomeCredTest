using HomeCredTest.Enums;

namespace HomeCredTest.Interfaces
{
    public interface IHCTask
    {
        int Id { get; }
        void Run();
        Priority Priority { get; set; }
        TaskCondition TaskCondition { get; set; }
    }
}
