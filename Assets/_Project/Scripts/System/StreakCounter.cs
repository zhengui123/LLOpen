/// <summary>
/// v1.5 连击计数（纯视觉，不改售价）。
/// </summary>
public class StreakCounter
{
    public int CurrentStreak { get; private set; }

    public void OnRoomRevealed(bool hasFlesh)
    {
        if (hasFlesh)
        {
            CurrentStreak++;
        }
        else
        {
            CurrentStreak = 0;
        }

        if (CurrentStreak > PlayerProgression.Instance.BestStreak)
        {
            PlayerProgression.Instance.BestStreak = CurrentStreak;
            PlayerProgression.Instance.Save();
        }

        EventBus.Publish(new StreakUpdatedEvent { Combo = CurrentStreak });
    }

    public int GetFlameLevel() => CurrentStreak switch
    {
        < 3 => 0,
        < 5 => 1,
        < 7 => 2,
        < 10 => 3,
        _ => 4
    };

    public void Reset()
    {
        CurrentStreak = 0;
        EventBus.Publish(new StreakUpdatedEvent { Combo = 0 });
    }
}
