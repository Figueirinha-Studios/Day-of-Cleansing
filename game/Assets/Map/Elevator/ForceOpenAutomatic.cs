using UnityEngine;

public class ForceOpenAutomatic : MonoBehaviour
{
    public ElevatorDoorController doorController;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MinhaFuncao();
        }
    }

    private void MinhaFuncao()
    {
        Debug.LogWarning("Forçando abertura das portas!");
        doorController.OpenDoors();
    }
}