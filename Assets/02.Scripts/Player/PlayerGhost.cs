using System.Collections;
using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
    [SerializeField] private float _activeTime = 0.5f; // 잔상이 유지되는 시간
    [SerializeField] private float _startAlpha = 0.5f; // 시작 투명도 (0~1)

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // 하얀색(255,255,255)이 아니라, 스프라이트 기본 색상을 가져옴
        _originalColor = _spriteRenderer.color;
    }

    public void SetGhost(Sprite sprite, bool flipX, Vector3 scale)
    {
        // 1. 플레이어의 현재 모습 복사
        _spriteRenderer.sprite = sprite;
        _spriteRenderer.flipX = flipX;
        transform.localScale = scale;

        // 2. 색상 초기화 (반투명하게 시작)
        Color color = _originalColor;
        color.a = _startAlpha;
        _spriteRenderer.color = color; // [수정] _originalColor가 아닌 color를 대입해야 함

        // 3. 서서히 사라지는 코루틴 시작
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;
        Color currentColor = _spriteRenderer.color;

        while (elapsed < _activeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(_startAlpha, 0f, elapsed / _activeTime);

            // 알파값만 변경
            currentColor.a = alpha;
            _spriteRenderer.color = currentColor;

            yield return null;
        }

        // 투명해지면 풀로 반납 (비활성화)
        gameObject.SetActive(false);
    }
}