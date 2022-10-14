using UnityEngine;

public class GhostKillArea : MonoBehaviour
{
    [SerializeField] private Transform _particleSystemTransform = default;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ghost = other.GetComponent<GhostController>();
        if (ghost)
        {
            ghost.OnCaptured(_particleSystemTransform);
        }
    }
}
