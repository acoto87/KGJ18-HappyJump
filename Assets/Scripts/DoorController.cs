using UnityEngine;

public class DoorController : MonoBehaviour
{
    private bool _opening;
    private bool _closing;
    private Animator _animator;
    private BoxCollider2D _collider;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        _animator.SetBool("opening", _opening);
        _animator.SetBool("closing", _closing);

        var gameController = GameObject.FindObjectOfType<GameController>();
        if (_opening)
        {
            _collider.enabled = false;
            gameController.PlaySound(GameSounds.DoorOpen);
        }
        else if (_closing)
        {
            _collider.enabled = true;
            gameController.PlaySound(GameSounds.DoorClose);
        }

        _opening = false;
        _closing = false;
    }

    public void Open()
    {
        _opening = true;
    }

    public void Close()
    {
        _closing = true;
    }
}
