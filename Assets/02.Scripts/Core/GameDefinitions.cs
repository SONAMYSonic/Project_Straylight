namespace IdolMasterFanGame
{
    // 게임 전반에서 사용할 속성 정의 (Vo, Da, Vi)
    public enum IdolMode
    {
        None = 0,
        Vocal,  // Red (Power)
        Dance,  // Blue (Speed)
        Visual  // Yellow (Utility)
    }

    // 모드 변경 이벤트를 위한 인터페이스 (의존성 역전 원칙 - DIP)
    public interface IModeChangeHandler
    {
        event System.Action<IdolMode> OnModeChanged;
        IdolMode CurrentMode { get; }
    }
}