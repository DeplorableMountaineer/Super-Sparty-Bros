using UnityEngine;

public class TimedObjectDestructor : MonoBehaviour
{
    public float timeOut = 1.0f;
    public bool detachChildren;

    // invote the DestroyNow funtion to run after timeOut seconds
    private void Awake()
    {
        Invoke("DestroyNow", timeOut);
    }

    // destroy the gameobject
    private void DestroyNow()
    {
        if (detachChildren) // detach the children before destroying if specified
            transform.DetachChildren();

        // destroy the game Object
        Destroy(gameObject);
    }
}