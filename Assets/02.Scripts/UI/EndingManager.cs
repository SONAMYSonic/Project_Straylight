using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndingManager : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _killText;
    [SerializeField] private TextMeshProUGUI _deathText;
    [SerializeField] private TextMeshProUGUI _totalScoreText;

    [Header("Grade Visuals")]
    [SerializeField] private Image _gradeImage; 
    [Tooltip("S, A, B, F 순서대로 등록 (0:S, 1:A, 2:B, 3:F)")]
    [SerializeField] private List<Sprite> _gradeSprites;

    [Header("Grade Thresholds")]
    [Tooltip("S등급 기준 점수")]
    [SerializeField] private int _gradeS = 9000;
    [Tooltip("A등급 기준 점수")]
    [SerializeField] private int _gradeA = 7000;
    [Tooltip("B등급 기준 점수")]
    [SerializeField] private int _gradeB = 4000;

    [Header("Score Calculation")]
    [Tooltip("클리어 보너스 점수")]
    [SerializeField] private int _clearBonus = 5000;
    [Tooltip("킬당 보너스 점수")]
    [SerializeField] private int _killBonus = 20;
    [Tooltip("1초당 감점")]
    [SerializeField] private int _timePenaltyPerSecond = 2;
    [Tooltip("1회 사망당 감점")]
    [SerializeField] private int _deathPenalty = 2000;

    [Header("Rolling Effect Settings")]
    [Tooltip("숫자 롤링 기본 지속 시간 (초)")]
    [SerializeField] private float _rollingDuration = 1.0f;
    [Tooltip("총점 롤링 지속 시간 (초)")]
    [SerializeField] private float _totalScoreRollingDuration = 1.5f;
    [Tooltip("연출 시작 전 대기 시간 (초)")]
    [SerializeField] private float _initialDelay = 0.5f;
    [Tooltip("도장 찍기 전 대기 시간 (초)")]
    [SerializeField] private float _preStampDelay = 0.5f;

    [Header("Stamp Effect Settings")]
    [Tooltip("도장 애니메이션 지속 시간 (초)")]
    [SerializeField] private float _stampDuration = 0.3f;
    [Tooltip("도장 시작 크기 배율")]
    [SerializeField] private float _stampStartScale = 3f;
    [Tooltip("도장 최종 크기 배율")]
    [SerializeField] private float _stampEndScale = 1f;
    [Tooltip("카메라 흔들림 강도")]
    [SerializeField] private float _cameraShakeIntensity = 3f;

    [Header("Audio")]
    [Tooltip("숫자 롤링 효과음")]
    [SerializeField] private AudioClip _rollingSFX;
    [Tooltip("도장 찍기 효과음")]
    [SerializeField] private AudioClip _stampSFX;
    [SerializeField] private AudioClip _backgroundMusic;

    private void Start()
    {
        if (_backgroundMusic != null)
        {
            SoundManager.Instance?.PlayBGM(_backgroundMusic);
        }

        _timeText.text = "00:00:00";
        _killText.text = "0";
        _deathText.text = "0";
        _totalScoreText.text = "0";
        _gradeImage.gameObject.SetActive(false);

        StartCoroutine(ResultSequenceRoutine());
    }

    private IEnumerator ResultSequenceRoutine()
    {
        // 저장된 플레이 시간 사용
        float playTime = GameSession.PlayTime;
        int kills = GameSession.TotalKills;
        int deaths = GameSession.DeathCount;
        int earnedScore = GameSession.TotalScore;

        // 최종 점수 계산
        int totalScore = CalculateTotalScore(earnedScore, kills, playTime, deaths);

        // 등급 결정 (사망 시 S등급 불가)
        Sprite finalGradeSprite = GetGradeSprite(totalScore, deaths);

        yield return new WaitForSeconds(_initialDelay);

        yield return StartCoroutine(RollingTimeRoutine(playTime));
        yield return StartCoroutine(RollingNumberRoutine(_killText, kills));
        yield return StartCoroutine(RollingNumberRoutine(_deathText, deaths));
        yield return StartCoroutine(RollingNumberRoutine(_totalScoreText, totalScore, _totalScoreRollingDuration));

        yield return new WaitForSeconds(_preStampDelay);

        yield return StartCoroutine(StampRoutine(finalGradeSprite));
    }

    private int CalculateTotalScore(int earnedScore, int kills, float playTime, int deaths)
    {
        int killScore = kills * _killBonus;
        int timePenalty = (int)playTime * _timePenaltyPerSecond;
        int deathPenalty = deaths * _deathPenalty;

        int totalScore = earnedScore + _clearBonus + killScore - timePenalty - deathPenalty;
        
        return Mathf.Max(0, totalScore);
    }

    private IEnumerator RollingTimeRoutine(float targetTime)
    {
        if (_rollingSFX != null)
        {
            SoundManager.Instance?.PlaySFX(_rollingSFX);
        }

        float elapsed = 0f;
        int maxRandomMinutes = Mathf.Max(10, Mathf.FloorToInt(targetTime / 60) + 5);
        
        while (elapsed < _rollingDuration)
        {
            elapsed += Time.deltaTime;
            int randomMin = Random.Range(0, maxRandomMinutes);
            int randomSec = Random.Range(0, 60);
            int randomMil = Random.Range(0, 100);
            _timeText.text = string.Format("{0:00}:{1:00}:{2:00}", randomMin, randomSec, randomMil);
            yield return null;
        }
        _timeText.text = FormatTime(targetTime);
    }

    private IEnumerator RollingNumberRoutine(TextMeshProUGUI textUI, int targetVal, float duration = -1f)
    {
        if (_rollingSFX != null)
        {
            SoundManager.Instance?.PlaySFX(_rollingSFX);
        }

        if (duration < 0) duration = _rollingDuration;
        
        int maxRandomValue = Mathf.Max(targetVal * 2, 100);
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int randomVal = Random.Range(0, maxRandomValue);
            textUI.text = randomVal.ToString();
            yield return null;
        }
        textUI.text = targetVal.ToString();
    }

    private IEnumerator StampRoutine(Sprite gradeSprite)
    {
        if (_gradeImage == null || gradeSprite == null) yield break;

        _gradeImage.sprite = gradeSprite;
        _gradeImage.gameObject.SetActive(true);
        _gradeImage.transform.localScale = Vector3.one * _stampStartScale;
        Color c = _gradeImage.color;
        c.a = 0f;
        _gradeImage.color = c;

        float elapsed = 0f;

        while (elapsed < _stampDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _stampDuration;
            float scale = Mathf.Lerp(_stampStartScale, _stampEndScale, t * t);
            float alpha = Mathf.Lerp(0f, 1f, t * 2f);
            _gradeImage.transform.localScale = Vector3.one * scale;
            c.a = Mathf.Clamp01(alpha);
            _gradeImage.color = c;
            yield return null;
        }

        _gradeImage.transform.localScale = Vector3.one * _stampEndScale;
        _gradeImage.color = Color.white;

        if (_stampSFX != null) SoundManager.Instance?.PlaySFX(_stampSFX);
        if (CameraShakeManager.Instance != null) CameraShakeManager.Instance.ShakeCamera(_cameraShakeIntensity);
    }

    private string FormatTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);
        int mil = Mathf.FloorToInt((time * 100) % 100);
        return string.Format("{0:00}:{1:00}:{2:00}", min, sec, mil);
    }

    private Sprite GetGradeSprite(int score, int deaths)
    {
        if (_gradeSprites == null || _gradeSprites.Count < 4) return null;

        // S등급만 무사망 필수, A/B/F는 점수로만 판정
        if (deaths == 0 && score >= _gradeS) return _gradeSprites[0]; // S
        if (score >= _gradeA) return _gradeSprites[1]; // A
        if (score >= _gradeB) return _gradeSprites[2]; // B
        return _gradeSprites[3]; // F
    }

    public void OnClickReturnTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}