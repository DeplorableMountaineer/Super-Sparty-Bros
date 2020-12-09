using UnityEngine;

public class EnemyStun : MonoBehaviour
{
    // if Player hits the stun point of the enemy, then call Stunned on the enemy
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // tell the enemy to be stunned
            GetComponentInParent<Enemy>().Stunned();

            //make the player bounce off enemy
            other.gameObject.GetComponent<CharacterController2D>().EnemyBounce();
        }
    }
}