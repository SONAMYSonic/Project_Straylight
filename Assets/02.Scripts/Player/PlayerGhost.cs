using System.Collections;
using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
    [SerializeField] private float _activeTime = 0.5f;
    [SerializeField] private float _startAlpha = 0.5f;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetGhost(Sprite sprite, bool flipX, Vector3 scale)
    {
        _spriteRenderer.sprite = sprite;
        _spriteRenderer.flipX = flipX;
        transform.localScale = scale;

        // Ghost 전용 색상 설정 (약간 푸르딩딩한 느낌 추천) or 원본 색상 유지
        // 여기서는 깔끔하게 흰색 베이스에 투명도만 줍니다.
        Color color = Color.white;
        color.a = _startAlpha;
        _spriteRenderer.color = color;

        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;
        Color currentColor = _spriteRenderer.color;

        while (elapsed < _activeTime)
        {
            elapsed += Time.deltaTime;
            // Lerp를 사용하여 부드럽게 투명해짐
            float alpha = Mathf.Lerp(_startAlpha, 0f, elapsed / _activeTime);

            currentColor.a = alpha;
            _spriteRenderer.color = currentColor;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}