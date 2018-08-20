using UnityEngine;

public class SpikesController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            var playerController = collider.GetComponent<PlayerController2D>();
            playerController.Die();
        }
    }
}
