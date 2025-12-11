using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _input;
    private PlayerMovement _movement;
    private PlayerCombat _combat; // [추가됨]

    void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>(); // [추가됨]

        if (_input == null) _input = gameObject.AddComponent<PlayerInput>();
        if (_movement == null) _movement = gameObject.AddComponent<PlayerMovement>();
        if (_combat == null) _combat = gameObject.AddComponent<PlayerCombat>(); // [추가됨]

        // 초기화
        _movement.Initialize(_input);
        _combat.Initialize(_input); // [추가됨]
    }

    void Update()
    {
        _movement.HandleDash();
        _combat.HandleAttack(); // [추가됨] 공격 로직 실행
    }

    void FixedUpdate()
    {
        _movement.HandleMovement();
    }
}