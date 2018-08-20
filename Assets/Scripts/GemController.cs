using UnityEngine;

public class GemController : MonoBehaviour
{
    public int value;
    public bool pickedUp;

    public ParticleSystem particles;

    private SpriteRenderer _renderer;
    private float _floatingFactor = 0.2f;
    private float _originalY;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _originalY = transform.position.y;
        _floatingFactor = Random.Range(0.1f, 0.3f);

    }

    void Update()
    {
        if (pickedUp)
        {
            _renderer.enabled = false;
        }
        else
        {
            _renderer.enabled = true;

            var position = transform.position;
            position.y = _originalY + _floatingFactor * Mathf.Sin(Time.time);
            transform.position = position;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            var player = collider.GetComponent<PlayerController2D>();

            var gameController = GameObject.FindObjectOfType<GameController>();
            if (gameController.Rewinding)
            {
                player.UnPickGem(value);
                pickedUp = false;

                particles.Play();
            }
            else
            {
                player.PickGem(value);
                pickedUp = true;

                particles.Play();
            }

            gameController.PlaySound(GameSounds.Pickup);
        }
    }
}
