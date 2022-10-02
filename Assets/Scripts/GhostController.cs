using UnityEngine;

public class GhostController : MonoBehaviour
{
    private bool _isHiding;
    private int _direction;

    [SerializeField] private SpriteRenderer _renderer = default;
    [SerializeField] private Animator _animator = default;

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        _renderer.flipX = _direction < 0;
        _animator.SetBool("IsHiding", _isHiding);
    }
}
