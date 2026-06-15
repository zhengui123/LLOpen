/// <summary>
/// 玩家运行时数据单例，各 Manager 通过 Instance 访问。
/// </summary>
public class PlayerData
{
    public static PlayerData Instance = new PlayerData();

    public int Gold { get; set; } = 200;
    public int DailyAdCount { get; set; } = 0;
    public float DailyBuff { get; set; } = 0f;
}
