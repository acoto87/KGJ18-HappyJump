using UnityEngine;

public class FlagController : MonoBehaviour
{
    public GameObject endText;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            var gameController = GameObject.FindObjectOfType<GameController>();
            gameController.Won = true;

            endText.SetActive(true);
        }
    }
}
