using UnityEngine;

public class Furniture : MonoBehaviour
{
    [SerializeField]
    private GhostController _ghostPrefab = default;

    public bool IsPossessed { get; private set; }
    public bool IsReserved { get; private set; }

    public void Reserve()
    {
        IsReserved = true;
    }

    public void CancelReservation()
    {
        IsReserved = false;
    }

    public void Possess()
    {
        if (IsPossessed)
        {
            return;
        }

        IsPossessed = true;
        IsReserved = true;
    }

    public void Exorcise()
    {
        Debug.Log("Exorcising " + this.gameObject.name);

        if (!IsPossessed)
        {
            return;
        }

        IsPossessed = false;
        IsReserved = false;
        var ghost = Instantiate<GhostController>(_ghostPrefab, transform.position, Quaternion.identity, transform);
        ghost.AppearedFrom = this;
    }
}
