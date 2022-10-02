using System.Linq;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    [SerializeField]
    private GameObject _particlePrefab = default;

    [SerializeField]
    private ParticleSystem _particleSystem = default;
   
    private ParticleSystem.Particle[] _particles;
    private GameObject[] _particleObjects;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _particleSystem.Play();
        } else
        if (Input.GetMouseButtonUp(0))
        {
            _particleSystem.Stop();
        }
    }

    private void Start()
    {
        _particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
        _particleObjects = ObjectPool.Instance.RequestInstances(_particlePrefab, _particleSystem.main.maxParticles).ToArray();
    }

    // Make the particles follow the gun's nozzle.
    void LateUpdate()
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

            _particleObjects[i].SetActive(true);
            _particleObjects[i].transform.position = position;
        }

        _particleSystem.SetParticles(_particles, particleCount);
    }
}
