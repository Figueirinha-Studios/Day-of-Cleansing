using UnityEngine;

public class UnlockCursor : MonoBehaviour
{
    public bool Lock;
    void Start()
    {
        if (Lock == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}