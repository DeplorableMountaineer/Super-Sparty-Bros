using UnityEngine;

public class Victory : MonoBehaviour
{
    public bool taken;
    public GameObject explosion;

    // if the player touches the victory object, it has not already been taken, and the player can move (not dead or victory)
    // then the player has reached the victory point of the level
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !taken &&
            other.gameObject.GetComponent<CharacterController2D>().playerCanMove)
        {
            // mark as taken so doesn't get taken multiple times
            taken = true;

            // if explosion prefab is provide, then instantiate it
            if (explosion)
            {
                Transform transform1 = transform;
                Instantiate(explosion, transform1.position, transform1.rotation);
            }

            // do the player victory thing
            other.gameObject.GetComponent<CharacterController2D>().Victory();

            // destroy the victory gameobject
            Destroy(gameObject);
        }
    }
}