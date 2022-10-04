using UnityEngine;

public class Furniture : MonoBehaviour
{
    [field: SerializeField]
    public bool IsOccupied { get; private set; }

    private GhostController _ghostController;

    public void Posess(GhostController ghostController)
    {
        Debug.Assert(ghostController, "Attempted to possess furniture with null ghost!");

        if (IsOccupied)
        {
            return;
        }

        IsOccupied = true;
        _ghostController = ghostController;
    }

    public void Exorcise()
    {
        if (!IsOccupied)
        {
            return;
        }

        IsOccupied = false;
        _ghostController.OnExorcised();
        _ghostController = null;
    }
}
