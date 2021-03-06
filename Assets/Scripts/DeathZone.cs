using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public bool destroyNonPlayerObjects = true;

    // Handle gameobjects collider with a deathzone object
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // if player then tell the player to do its FallDeath
            other.gameObject.GetComponent<CharacterController2D>().FallDeath();
        }
        else if (destroyNonPlayerObjects)
        {
            // not player so just kill object - could be falling enemy for example
            Destroy(other.gameObject);
        }
    }
}