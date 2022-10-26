using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Tilemaps;
//using UnityEngine.U2D;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap = default;

    [SerializeField] private Transform _target = default;
    public Transform Target
    {
        get => _target;
        set => _target = value;
    }
    [SerializeField] private Vector2 _targetFollowOffset = default;

    [SerializeField] private float _stopDistance = 0.1F;
    [SerializeField] private float _followSpeed = 1F;
    [SerializeField] private float _lerpTime = 0.2F;

    private Camera _camera = default;
    private PixelPerfectCamera _ppCamera = default;

    private bool _isStopped = false;
    private float _currentSpeed = 0;
    private Vector2 _currPosition;
    private Vector2? _targetPosition = null;
    private float _targetZ;

    private void Awake()
    {
        _camera = transform.GetComponent<Camera>();
        if (!_camera)
        {
            Debug.LogError("Missing \"Camera\" component!");
        }
        _ppCamera = transform.GetComponent<PixelPerfectCamera>();
        if (!_ppCamera)
        {
            Debug.LogError("Missing \"PixelPerfectCamera\" component!");
        }
    }

    private void FixedUpdate()
    {
        _currPosition = new Vector2(_camera.transform.position.x, _camera.transform.position.y);

        if (_targetPosition.HasValue && Vector2.Distance(_targetPosition.Value, _currPosition) < _stopDistance)
        {
            _isStopped = true;
            _currentSpeed = 0;
        }
        else
        {
            _isStopped = false;
            _currentSpeed += Time.fixedDeltaTime * (_followSpeed / _lerpTime);
            if (_currentSpeed > _followSpeed)
            {
                _currentSpeed = _followSpeed;
            }
        }

        if (_targetPosition.HasValue)
        {
            MoveTowards();
        }
    }

    private void LateUpdate()
    {
        if (_target)
        {
            var levelArea = _tilemap.localBounds;
            var halfViewSize = (new Vector2(
                _camera.pixelWidth,
                _camera.pixelHeight) / _ppCamera.assetsPPU) * 0.5F;

            float clampedX = Mathf.Clamp(
                _target.position.x + _targetFollowOffset.x,
                levelArea.min.x + halfViewSize.x,
                levelArea.max.x - halfViewSize.x);
            float clampedY = Mathf.Clamp(
                _target.position.y + _targetFollowOffset.y,
                levelArea.min.y + halfViewSize.y,
                levelArea.max.y - halfViewSize.y);

            _targetPosition = new Vector2(clampedX, clampedY);
            _targetZ = _target.position.z - 100;
        }
        else
        {
            _targetPosition = null;
        }
    }

    private void MoveTowards()
    {
        var fromTo = _targetPosition.Value - _currPosition;
        var dst = fromTo.magnitude;
        var dir = fromTo.normalized;

        var delta = _currentSpeed * Time.fixedDeltaTime;
        if (delta > dst)
        {
            delta = dst;
        }

        _camera.transform.position = new Vector3(
            _camera.transform.position.x + dir.x * delta,
            _camera.transform.position.y + dir.y * delta,
            _targetZ);
    }

    public void Teleport()
    {
        var levelArea = _tilemap.localBounds;
        var halfViewSize = (new Vector2(
            _camera.pixelWidth,
            _camera.pixelHeight) / _ppCamera.assetsPPU) * 0.5F;

        float clampedX = Mathf.Clamp(
            _target.position.x + _targetFollowOffset.x,
            levelArea.min.x + halfViewSize.x,
            levelArea.max.x - halfViewSize.x);
        float clampedY = Mathf.Clamp(
            _target.position.y + _targetFollowOffset.y,
            levelArea.min.y + halfViewSize.y,
            levelArea.max.y - halfViewSize.y);

        _targetPosition = new Vector2(clampedX, clampedY);
        _camera.transform.position = new Vector3(
            clampedX,
            clampedY,
            _targetZ);
    }

    private void OnDrawGizmos()
    {
        if (_targetPosition.HasValue)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(_targetPosition.Value.x, _targetPosition.Value.y), 0.1F);
        }
    }
}
