using UnityEngine;

public class GhostController : MonoBehaviour
{
    private enum GhostState
    {
        HIDING,
        FLEEING,
        LOOKING_FOR_HIDING_SPOT,
    }

    [SerializeField] private float _slowSpeed = default;
    [SerializeField] private float _fastSpeed = default;

    public bool IsVisible => _isVisible;
    public bool IsHiding => _isHiding;

    private bool _isVisible;
    private bool _isHiding;
    private Vector2 _direction;

    [SerializeField] private SpriteRenderer _renderer = default;
    [SerializeField] private Animator _animator = default;

    [SerializeField] private Rigidbody2D _rb2d = default;

    private Transform _player;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        var transparentColor = _renderer.color;
        transparentColor.a = 0.2F;
        _renderer.color = transparentColor;
    }

    private void Update()
    {
        _renderer.flipX = _direction.x < 0;
        _animator.SetBool("IsHiding", _isHiding);
    }

    private void FixedUpdate()
    {
        if (_isHiding)
        {
            DoHiding();
        }
        else
        {
            DoFleeing();
        }
    }

    private void DoHiding()
    {
        // Just hides and moves objects around every so often.
    }

    private void DoFleeing()
    {
        // Tries to run away from the player and find another object to hide in.
        //_direction = (transform.position - _player.position).normalized;
        //_rb2d.velocity = _direction * _slowSpeed;
        _rb2d.velocity = Vector2.zero;
        if (_isBeingSucked)
        {
            _rb2d.velocity += _suckVelocity;
        }
    }

    private bool _isBeingSucked;
    private Vector2 _suckVelocity;

    public void OnSuckStarted()
    {
        Debug.Log("Suck started!");

        _isBeingSucked = true;
        _isHiding = false;

        var opaqueColor = _renderer.color;
        opaqueColor.a = 1;
        _renderer.color = opaqueColor;
    }

    public void SuckTowards(Vector2 velocity)
    {
        _suckVelocity = velocity;
    }

    public void OnSuckStopped()
    {
        Debug.Log("Suck stopped!");

        _isBeingSucked = false;
    }

    public void OnExorcised()
    {
        _isHiding = false;
        _isVisible = true;

        var opaqueColor = _renderer.color;
        opaqueColor.a = 1;
        _renderer.color = opaqueColor;
    }
}
