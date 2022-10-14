using UnityEngine;

public class ZOrderSetter : MonoBehaviour
{
    private void LateUpdate()
    {
        var pos = this.transform.position;
        this.transform.position = new Vector3(
            pos.x,
            pos.y,
            pos.y);
    }
}
