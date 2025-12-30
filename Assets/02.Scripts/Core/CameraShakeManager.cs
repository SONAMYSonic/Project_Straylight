using UnityEngine;
using Unity.Cinemachine; // 유니티 6 (Cinemachine 3.x) 기준 네임스페이스
// 만약 에러가 나면 using Cinemachine; 으로 바꿔보세요 (2.x 버전)

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    private CinemachineImpulseSource _impulseSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 컴포넌트가 없으면 자동으로 추가
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (_impulseSource == null)
            _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
    }

    public void ShakeCamera(float intensity, float time = 0.2f)
    {
        // 1. 흔들림 강도 설정 (Velocity)
        // 랜덤한 방향으로 힘을 줍니다.
        Vector3 shakeVelocity = Random.insideUnitCircle.normalized * intensity;

        // 2. 임펄스 발생!
        _impulseSource.GenerateImpulseWithVelocity(shakeVelocity);

        // *참고: 지속 시간(time)은 Impulse Listener의 Damping이나 
        // Impulse Definition의 Envelope 설정에 따라 달라지는데,
        // 코드로 간단히 제어하려면 Cinemachine설정을 만져야 해서
        // 일단은 "한 방 쾅!" 때리는 걸로 구현합니다.
    }
}