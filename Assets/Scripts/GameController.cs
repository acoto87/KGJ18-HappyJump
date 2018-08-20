using UnityEngine;

public class GameController : MonoBehaviour
{
    public bool Rewinding;
    public bool Won;

    public AudioClip[] buttonClips;
    public AudioClip[] doorClips;
    public AudioClip[] hurtClips;
    public AudioClip[] jumpClips;
    public AudioClip[] pickupClips;

    private PlayerController2D _player;
    private AudioSource _audioSource;

    void Start()
    {
        _player = GameObject.FindObjectOfType<PlayerController2D>();
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

#if UNITY_ANDROID
        var screenWidth = Screen.width;
        for (int i = 0; i < Input.touchCount; i++)
        {
            var touch = Input.touches[i];
            var position = touch.position;
            if (position.x < screenWidth / 2)
            {
                Rewinding = touch.phase == TouchPhase.Began ||
                            touch.phase == TouchPhase.Moved ||
                            touch.phase == TouchPhase.Stationary;
                break;
            }
        }
#else
        Rewinding = Input.GetKey(KeyCode.R);
#endif

    }

    public void PlaySound(GameSounds sound)
    {
        AudioClip clip = null;

        switch (sound)
        {
            case GameSounds.Button:
                clip = buttonClips[Random.Range(0, buttonClips.Length)];
                break;

            case GameSounds.DoorOpen:
                clip = doorClips[0];
                break;

            case GameSounds.DoorClose:
                clip = doorClips[1];
                break;

            case GameSounds.Hurt:
                clip = hurtClips[Random.Range(0, hurtClips.Length)];
                break;

            case GameSounds.Jump:
                clip = jumpClips[Random.Range(0, jumpClips.Length)];
                break;

            case GameSounds.Pickup:
                clip = pickupClips[Random.Range(0, pickupClips.Length)];
                break;
        }

        if (clip != null)
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();

            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
}

public enum GameSounds
{
    Button,
    DoorOpen,
    DoorClose,
    Hurt,
    Jump,
    Pickup
}

