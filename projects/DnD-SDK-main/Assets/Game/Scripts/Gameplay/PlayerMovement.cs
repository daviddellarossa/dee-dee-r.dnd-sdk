using UnityEngine;
using UnityEngine.InputSystem;

namespace DeeDeeR.DnD.Game.Gameplay
{
    /// <summary>
    /// Simple WASD / gamepad-stick movement using CharacterController.
    /// Reads the <c>Player/Move</c> action from the project's Input System asset.
    /// No combat or interaction logic — exists only to prove the game loop works end-to-end.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private PlayerInput _playerInput;

        private InputAction _moveAction;
        private CharacterController _cc;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (_playerInput == null)
                _playerInput = GetComponentInParent<PlayerInput>();

            if (_playerInput != null)
            {
                _moveAction = _playerInput.actions["Move"];
                _moveAction?.Enable();
            }
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
        }

        private void Update()
        {
            if (_moveAction == null) return;

            Vector2 input = _moveAction.ReadValue<Vector2>();
            Vector3 move  = new Vector3(input.x, 0f, input.y) * (_speed * Time.deltaTime);
            _cc.Move(move);
        }
    }
}
