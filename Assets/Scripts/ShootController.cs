using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    [SerializeField]
    private GameObject _particlePrefab = default;
    private ParticleSystem.Particle[] _particles;
    private GameObject[] _particleObjects;
    [SerializeField]
    private ParticleSystem _particleSystem = default;

    [SerializeField]
    private LayerMask _suckLayer = default;
    [SerializeField]
    private float _suckRange = default;
    [SerializeField]
    private float _suckWidth = default;
    [SerializeField]
    private float _suckSpeed = 1.5F;

    private void Start()
    {
        _particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
        _particleObjects = ObjectPool.Instance.RequestInstances(_particlePrefab, _particleSystem.main.maxParticles).ToArray();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSucking();
        } else
        if (Input.GetMouseButtonUp(0))
        {
            StopSucking();
        }
    }

    // Make the particles follow the gun's nozzle.
    private void LateUpdate()
    {
        foreach (var particleObject in _particleObjects)
        {
            particleObject.SetActive(false);
        }

        var particleCount = _particleSystem.GetParticles(_particles);

        for (int i = 0; i < particleCount; ++i)
        {
            var fromToDir = _particleSystem.transform.position - _particles[i].position;
            var magnitude = _particles[i].velocity.magnitude;
            _particles[i].velocity = fromToDir.normalized * magnitude;
            var position = _particles[i].position;
            _particles[i].position = new Vector3(position.x, position.y, _particleSystem.transform.position.z);

            // The reason for _particleObject's existance is so that we can render
            // the particles above/below other objects based on their Y coordinates.
            _particleObjects[i].SetActive(true);
            _particleObjects[i].transform.position = position;
        }

        _particleSystem.SetParticles(_particles, particleCount);
    }

    private Coroutine _suckCoroutine;
    private List<GhostController> _ghostsBeingSucked;

    private void StartSucking()
    {
        _particleSystem.Play();
        if (_ghostsBeingSucked == null)
        {
            _ghostsBeingSucked = new List<GhostController>();
        }
        _suckCoroutine = StartCoroutine(Suck());
    }

    private void StopSucking()
    {
        _particleSystem.Stop();
        StopCoroutine(_suckCoroutine);
        foreach (var ghost in _ghostsBeingSucked)
        {
            ghost.OnSuckStopped();
        }
        _ghostsBeingSucked.Clear();
    }

    private IEnumerator Suck()
    {
        while (true)
        {
            var bottomLeft = _particleSystem.transform.position
                + _particleSystem.transform.up * _suckWidth * 0.5F;
            var topRight = _particleSystem.transform.position
                + _particleSystem.transform.forward * _suckRange
                + _particleSystem.transform.up * -_suckWidth * 0.5F;

            Debug.DrawLine(bottomLeft, topRight);

            var overlappedObjects = Physics2D.OverlapAreaAll(bottomLeft, topRight, _suckLayer.value);

            var newGhosts = overlappedObjects
                .Select(x => x.GetComponent<GhostController>())
                .Where(x => x)
                .ToList();

            var ghostsStartedSucking = newGhosts.Except(_ghostsBeingSucked);
            var ghostsStoppedSucking = _ghostsBeingSucked.Except(newGhosts);

            foreach (var ghost in ghostsStoppedSucking)
            {
                ghost.OnSuckStopped();
            }

            foreach (var ghost in ghostsStartedSucking)
            {
                ghost.OnSuckStarted();
            }

            foreach (var ghost in newGhosts)
            {
                var fromTo = _particleSystem.transform.position - ghost.transform.position;
                var distance = fromTo.magnitude;
                var suckStrength = Mathf.Lerp(1, 0.333F, Mathf.Clamp01(distance / _suckRange));
                ghost.SuckTowards(fromTo.normalized * suckStrength * _suckSpeed);
            }

            _ghostsBeingSucked = newGhosts;

            yield return new WaitForSeconds(0.05F);
        }
    }
}
