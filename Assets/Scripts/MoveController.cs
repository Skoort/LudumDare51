using UnityEngine;

public class MoveController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3F;

    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rb2d;

    public bool CanMove { get; set; } = true;

    private void Awake()
    {
        if (!_rb2d)
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }

        if (!_animator)
        {
            _animator = GetComponent<Animator>();
        }
    }

    private Vector3 _moveDir;
    private void Update()
    {
        if (CanMove)
        {
            _moveDir = new Vector3(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"));

            var isMoving = _moveDir.magnitude > 0.1F;
            _animator.SetBool("IsMoving", isMoving);
        }
    }

    private void FixedUpdate()
    {
        if (CanMove)
        {
            var clampedMoveDir = Vector3.ClampMagnitude(_moveDir, 1);

            _rb2d.velocity = clampedMoveDir * _moveSpeed;
        }
        else
        {
            _rb2d.velocity = Vector2.zero;
        }
    }
}
