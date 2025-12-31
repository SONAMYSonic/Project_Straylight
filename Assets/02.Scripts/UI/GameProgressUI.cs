using UnityEngine;
using UnityEngine.UI;

public class GameProgressUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image _fillImage; // 게이지바 이미지 (Filled 타입이어야 함)

    [Header("Sprites")]
    [SerializeField] private Sprite _whiteSprite;  // 일반 웨이브 (흰색)
    [SerializeField] private Sprite _lemonSprite;  // 중간 보스 (레몬색)
    [SerializeField] private Sprite _orangeSprite; // 최종 보스 (주황색)

    /// <summary>
    /// 진행도 및 상태 업데이트
    /// </summary>
    /// <param name="progress">0.0f ~ 1.0f 사이의 진행률</param>
    /// <param name="state">0: 일반, 1: 중간보스, 2: 최종보스</param>
    public void UpdateProgress(float progress, int state)
    {
        if (_fillImage == null) return;

        // 1. 진행도 반영
        _fillImage.fillAmount = progress;

        // 2. 상태에 따른 색상/스프라이트 변경
        switch (state)
        {
            case 0: // 일반 웨이브
                if (_whiteSprite != null) _fillImage.sprite = _whiteSprite;
                _fillImage.color = Color.white;
                break;
            case 1: // 중간 보스 등장
                if (_lemonSprite != null) _fillImage.sprite = _lemonSprite;
                _fillImage.color = Color.white; // 스프라이트 본연의 색 사용
                break;
            case 2: // 최종 보스 등장
                if (_orangeSprite != null) _fillImage.sprite = _orangeSprite;
                _fillImage.color = Color.white;
                break;
        }
    }
}