using System.Collections;

namespace DDARoguelike
{
    public interface IBossPattern
    {
        string PatternName { get; }
        float Weight { get; }
        float RecoveryDuration { get; }

        bool CanExecute(BossContext context);
        IEnumerator Execute(BossContext context);
    }
}
