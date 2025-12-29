using UnityEngine;

public class InfiniteMap : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _tileSize = 40f; // 타일맵 한 덩어리의 크기 (가로/세로 동일 가정)

    private Vector3 _startPosition;

    private void Start()
    {
        if (_playerTransform == null)
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        _startPosition = transform.position;
    }

    private void LateUpdate()
    {
        // 1. 플레이어가 원점에서 얼마나 이동했는지 계산
        float xDiff = _playerTransform.position.x - _startPosition.x;
        float yDiff = _playerTransform.position.y - _startPosition.y;

        // 2. 타일 크기만큼 이동했으면, 배경의 기준점을 옮김 (뚝뚝 끊겨 이동)
        if (Mathf.Abs(xDiff) >= _tileSize)
        {
            _startPosition.x += Mathf.Sign(xDiff) * _tileSize;
            transform.position = _startPosition; // 실제 오브젝트 이동
        }

        if (Mathf.Abs(yDiff) >= _tileSize)
        {
            _startPosition.y += Mathf.Sign(yDiff) * _tileSize;
            transform.position = _startPosition;
        }
    }
}