using UnityEngine;

public class LookController : MonoBehaviour
{
    private Camera _camera;

    [SerializeField] private SpriteRenderer _playerRenderer = default;
    [SerializeField] private SpriteRenderer _gunRenderer = default;

    [SerializeField] private Transform _gunRoot = default;
    [SerializeField] private Transform _lookLeftGunRoot = default;
    [SerializeField] private Transform _lookRightGunRoot = default;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        var lookPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
        var lookDir = lookPoint - transform.position;
        lookDir.z = 0;
        _gunRoot.right = lookDir.normalized;
        if (lookDir.x < 0)
        {
            _gunRoot.parent = _lookLeftGunRoot;
        }
        else
        {
            _gunRoot.parent = _lookRightGunRoot;
        }
        _gunRoot.localPosition = Vector3.zero;
        _gunRenderer.flipY = lookDir.x < 0;

        _playerRenderer.flipX = lookDir.x < 0;
    }
}
