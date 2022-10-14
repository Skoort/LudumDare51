using System.Collections;
using System.Linq;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    private enum GhostState
    {
        FURNITURE_TRANSITION,
        LOOKING_FOR_HIDING_SPOT,
        DEATH
    }

    [SerializeField] private float _slowSpeed = default;
    [SerializeField] private float _fastSpeed = default;
    [SerializeField] private AnimationCurve _fleeStrengthOverDistance = default;
    [SerializeField] private float _minimumHidingSpotDistance = 7;

    [SerializeField] private SpriteRenderer _renderer = default;
    [SerializeField] private Animator _animator = default;
    [SerializeField] private Rigidbody2D _rb2d = default;


    [SerializeField] private GhostState _state;
    private Vector2 _direction;
    private Transform _player;
    private Transform _hidingSpot;
    public Furniture AppearedFrom { get; set; }

    private void Awake()
    {
        _state = GhostState.FURNITURE_TRANSITION;

        _player = GameObject.FindGameObjectWithTag("Player").transform;

        _findHidingSpotCoroutine = StartCoroutine(FindHidingSpot());
    }

    private void Update()
    {
        if (_state == GhostState.DEATH || _state == GhostState.FURNITURE_TRANSITION)
        {
            return;
        }

        _renderer.flipX = _direction.x < 0;
    }

    private void PlayAnimationThenDestroy(string animationTrigger)
    {
        _animator.SetTrigger(animationTrigger);
        // TODO: Does setting the trigger instantly transition to the death clip?
        var clipLength = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        Destroy(this.gameObject, clipLength);
    }

    private void FixedUpdate()
    {
        if (_state == GhostState.DEATH || _state == GhostState.FURNITURE_TRANSITION)
        {
            return;
        }

        Debug.Log("Fleeing!");

        Vector2 hideDirection = Vector2.zero;
        if (_hidingSpot)
        {
            Vector2 hideFromTo = _hidingSpot.position - transform.position;
            var distance = hideFromTo.magnitude;
            if (distance < 0.1F)
            {
                _hidingSpot.GetComponent<Furniture>().Possess();
                PlayAnimationThenDestroy("OnHidden");
                _state = GhostState.FURNITURE_TRANSITION;
                _rb2d.velocity = Vector2.zero;
                return;
            }
            hideDirection = hideFromTo / distance;
        }

        Vector2 fleeFromTo = transform.position - _player.position;
        Vector2 fleeDirection = fleeFromTo.normalized;

        if (_isBeingSucked)
        {
            _direction = fleeDirection;
            _rb2d.velocity = fleeDirection * _slowSpeed + _suckVelocity;
        }
        else
        {
            var fleeStrength = _fleeStrengthOverDistance.Evaluate(fleeFromTo.magnitude);
            // TODO: Add tie breaker in case opposite. Choose random direction, preferably one where center of furniture mass is greater.

            _direction = Vector2.Lerp(hideDirection, fleeDirection, fleeStrength).normalized;
            _rb2d.velocity = _direction * _slowSpeed;
        }
    }

    #region --- ANIMATION CALLBACKS ---

    public void OnAppeared()
    {
        _state = GhostState.LOOKING_FOR_HIDING_SPOT;
    }

    #endregion

    #region --- HOOVER CALLBACKS ---

    private bool _isBeingSucked;
    private Vector2 _suckVelocity;

    public void OnSuckStarted()
    {
        Debug.Log("Suck started!");

        _animator.SetBool("IsBeingSucked", true);
        _isBeingSucked = true;

        //var opaqueColor = _renderer.color;
        //opaqueColor.a = 1;
        //_renderer.color = opaqueColor;
    }

    public void SuckTowards(Vector2 velocity)
    {
        _suckVelocity = velocity;
    }

    public void OnSuckStopped()
    {
        Debug.Log("Suck stopped!");

        _animator.SetBool("IsBeingSucked", false);
        _isBeingSucked = false;
    }

    public void OnCaptured(Transform _particleSystemTransform)
    {
        if (_state == GhostState.DEATH)
        {
            return;
        }

        transform.parent = _particleSystemTransform;
        _rb2d.velocity = Vector2.zero;

        if (_findHidingSpotCoroutine != null)
        {
            StopCoroutine(_findHidingSpotCoroutine);
        }
        PlayAnimationThenDestroy("OnCaptured");
    }

    #endregion

    private Coroutine _findHidingSpotCoroutine;

    private IEnumerator FindHidingSpot()
    {
        while (true)
        {
            Furniture closestFurniture = null;
            var closestDistance = Mathf.Infinity;

            var furniture = GameObject.FindObjectsOfType<Furniture>();
            foreach (var piece in furniture)
            {
                if (piece == AppearedFrom || piece.IsReserved)
                {
                    continue;
                }

                var distanceToPlayer = Vector2.Distance(piece.transform.position, _player.position);
                if (distanceToPlayer < _minimumHidingSpotDistance)
                {
                    continue;
                }

                var distanceToGhost = Vector2.Distance(transform.position, piece.transform.position);
                if (distanceToGhost < closestDistance)
                {
                    closestDistance = distanceToGhost;
                    closestFurniture = piece;
                }
            }

            _hidingSpot = closestFurniture.transform;

            yield return new WaitForSeconds(0.5F);
        }
    }

    private void OnDrawGizmos()
    {
        var lineDistance = _rb2d.velocity.magnitude * 0.5F;

        if (_player)
        {
            Debug.DrawRay(
                (Vector2) transform.position,
                (Vector2) (transform.position - _player.position).normalized * (lineDistance + 0.1F),
                Color.red);
        }

        if (_hidingSpot)
        {
            Debug.DrawLine(
                (Vector2) transform.position,
                (Vector2) _hidingSpot.position,
                Color.green);
        }

        Debug.DrawRay(
            (Vector2) transform.position,
            (Vector2) _rb2d.velocity.normalized * lineDistance,
            Color.yellow);
    }
}
