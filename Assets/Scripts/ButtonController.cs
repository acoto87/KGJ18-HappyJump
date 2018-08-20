using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public DoorController[] targetDoors;
    public ButtonAction action;

    private Animator _animator;
    private bool _pushed;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        _animator.SetBool("pushed", _pushed);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            var gameController = GameObject.FindObjectOfType<GameController>();
            if (gameController.Rewinding)
            {
                _pushed = false;

                for (int i = 0; i < targetDoors.Length; i++)
                {
                    if (action == ButtonAction.Close)
                        targetDoors[i].Open();
                    else
                        targetDoors[i].Close();
                }
            }
            else
            {
                _pushed = true;

                for (int i = 0; i < targetDoors.Length; i++)
                {
                    if (action == ButtonAction.Close)
                        targetDoors[i].Close();
                    else
                        targetDoors[i].Open();
                }

            }

            gameController.PlaySound(GameSounds.Button);
        }
    }
}

public enum ButtonAction
{
    Open,
    Close
}

