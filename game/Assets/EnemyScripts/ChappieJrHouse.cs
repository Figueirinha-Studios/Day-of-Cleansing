
using UnityEngine;

public class ChappieJrHouse : MonoBehaviour
{
    [Header("Entrada da casinha")]
    public Transform entrancePoint;

    [Header("Tempo escondido")]
    public float hideDuration = 7f;

    [Header("Estado")]
    public bool isOccupied = false;

    public Vector3 EntrancePosition
    {
        get
        {
            return entrancePoint != null
                ? entrancePoint.position
                : transform.position;
        }
    }
}
