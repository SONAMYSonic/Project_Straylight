public static class GameSession
{
    public static int DeathCount = 0;   // 죽은 횟수
    public static float StartTime = 0f; // 시작 시간
    public static int TotalKills = 0;   // 잡은 몬스터 수 (순수 마리 수)
    public static int TotalScore = 0;   // [추가] 획득한 총 점수 (몬스터별 점수 합산)

    public static void ResetSession()
    {
        DeathCount = 0;
        StartTime = UnityEngine.Time.time;
        TotalKills = 0;
        TotalScore = 0; // [추가] 초기화
    }
}