using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IdolMasterFanGame.UI
{
    public class PlayerModeUI : MonoBehaviour
    {
        [Serializable]
        public struct ModeUIData
        {
            public IdolMode Mode;
            public Sprite IconSprite;
            // 필요 시 Color ThemeColor; 등 추가 가능
        }

        [Header("UI References")]
        [SerializeField] private Image _targetImage;

        [Header("Data Configuration")]
        [SerializeField] private List<ModeUIData> _uiDataList;

        // 성능 최적화: 리스트 대신 딕셔너리로 O(1) 접근
        private Dictionary<IdolMode, ModeUIData> _dataMap;

        private void Awake()
        {
            InitializeDataMap();
        }

        private void InitializeDataMap()
        {
            _dataMap = new Dictionary<IdolMode, ModeUIData>();

            foreach (var data in _uiDataList)
            {
                if (!_dataMap.ContainsKey(data.Mode))
                {
                    _dataMap.Add(data.Mode, data);
                }
            }
        }

        // 이벤트 리스너 메서드
        public void UpdateModeUI(IdolMode newMode)
        {
            if (_targetImage == null) return;

            if (_dataMap.TryGetValue(newMode, out ModeUIData data))
            {
                _targetImage.sprite = data.IconSprite;
            }
            else
            {
                // None이거나 데이터가 없을 때의 처리 (투명하게 하거나 기본값)
                Debug.LogWarning($"[PlayerModeUI] UI Data not found for: {newMode}");
            }
        }
    }
}