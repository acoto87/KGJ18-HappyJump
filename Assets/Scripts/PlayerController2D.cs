using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerController2D : MonoBehaviour
{
    private CharacterController2D _controller;
    private SpriteRenderer _spriteRenderer;

    private Animator _animator;

    public float speed = 6.0f;
    public float maxJumpHeight = 4.0f;
    public float minJumpHeight = 1.0f;
    public float jumpTime = 0.4f;

    public float accelerationTimeAir = 0.2f;
    public float accelerationTimeGrounded = 0.1f;

    public Text rewindText;
    public Text rewindTextShadow;

    private float _gravity;
    private float _maxJumpVelocity;
    private float _minJumpVelocity;

    private Vector2 _velocity;
    private float _velocityXSmoothing;

    private int _value;
    private bool _isDead;
    private Stack<PlayerState> _states;

    // allow ~5 min of gameplay in the buffer of states before it has to resize at 60fps
    private const int _statesInitialCapactiy = 5 * 60 * 60;

    void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _gravity = (-2 * maxJumpHeight) / (jumpTime * jumpTime);
        _maxJumpVelocity = Mathf.Abs(_gravity) * jumpTime;
        _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);

        _states = new Stack<PlayerState>(_statesInitialCapactiy);

        Debug.Log("Gravity: " + _gravity);
        Debug.Log("Jump velocity: " + _maxJumpVelocity);
    }

    void Update()
    {
        var gameController = GameObject.FindObjectOfType<GameController>();
        if (gameController.Won)
        {
            _animator.SetBool("win", true);
            return;
        }

        if (gameController.Rewinding)
        {
            Rewind();
            return;
        }

        if (!_isDead)
        {
            var input = new Vector2(1.0f, 0);
            var scale = _spriteRenderer.transform.localScale;
            scale.x = _controller._facingDirection >= 0 ? 1 : -1;
            _spriteRenderer.transform.localScale = scale;

#if UNITY_ANDROID
            var jumpingDown = false;
            var jumpingUp = false;

            if (Input.touchCount > 0)
            {
                var screenWidth = Screen.width;

                var touch = Input.touches[0];
                var position = touch.position;
                if (position.x >= screenWidth / 2)
                {
                    if (touch.phase == TouchPhase.Began)
                        jumpingDown = true;
                    else if (touch.phase == TouchPhase.Ended)
                        jumpingUp = true;
                }
            }
#else
            var jumpingDown = Input.GetButtonDown("Jump");
            var jumpingUp = Input.GetButtonUp("Jump");
#endif

            if (jumpingDown && _controller.collisions.below)
            {
                if (_controller.collisions.slidingDownMaxSlope)
                {
                    // not jumping against the max slope
                    if (Mathf.Sign(input.x) != -Mathf.Sign(_controller.collisions.slopeNormal.x))
                    {
                        _velocity.x = _maxJumpVelocity * _controller.collisions.slopeNormal.x;
                        _velocity.y = _maxJumpVelocity * _controller.collisions.slopeNormal.y;
                    }
                }
                else
                {
                    _velocity.y = _maxJumpVelocity;
                }

                gameController.PlaySound(GameSounds.Jump);
            }

            if (jumpingUp)
            {
                if (_velocity.y > _minJumpVelocity)
                {
                    _velocity.y = _minJumpVelocity;
                }
            }

            var running = input.x != 0;
            var jumping = _velocity.y != 0;

            _animator.SetBool("running", running);
            _animator.SetBool("jumping", jumping);
            _animator.SetBool("dead", _isDead);

            var targetVelocityX = input.x * speed;
            var smoothTime = _controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAir;
            _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing, smoothTime);

            _velocity.y += _gravity * Time.deltaTime;

            _controller.Move(_velocity * Time.deltaTime, input);

            if (_controller.collisions.above || _controller.collisions.below)
            {
                if (_controller.collisions.slidingDownMaxSlope)
                {
                    _velocity.y += _controller.collisions.slopeNormal.y * -_gravity * Time.deltaTime;
                }
                else
                {
                    _velocity.y = 0;
                }
            }

            if (!_controller.collisions.left &&
                !_controller.collisions.right)
            {
                _states.Push(new PlayerState()
                {
                    position = transform.position,
                    jumping = jumping,
                    running = running
                });
            }
        }

        //PrintDebugInfo(input);
    }

    private void Rewind()
    {
        if (_states.Count > 0)
        {
            _isDead = false;

            var state = _states.Pop();

            transform.position = state.position;
            _animator.SetBool("running", state.running);
            _animator.SetBool("jumping", state.jumping);
            _animator.SetBool("dead", _isDead);

            rewindText.transform.parent.gameObject.SetActive(false);

        }
    }

    public void PickGem(int value)
    {
        _value += value;
    }

    public void UnPickGem(int value)
    {
        _value -= value;
    }

    public void Die()
    {
        _isDead = true;
        _animator.SetBool("dead", _isDead);

        var gameController = GameObject.FindObjectOfType<GameController>();
        gameController.PlaySound(GameSounds.Hurt);

#if UNITY_ANDROID
        rewindText.text = "Touch left side of the screen to rewind";
        rewindTextShadow.text = "Touch left side of the screen to rewind";
#else
        rewindText.text = "Press R to rewind";
        rewindTextShadow.text = "Press R to rewind";
#endif
        rewindText.transform.parent.gameObject.SetActive(true);
    }

    private void PrintDebugInfo(Vector2 input)
    {
        var debugInfo = new StringBuilder();
        debugInfo.AppendLine("Player Info");

        debugInfo.AppendFormat("input: {0}", input);
        debugInfo.AppendLine();

        debugInfo.AppendFormat("velocity: {0}", _velocity);
        debugInfo.AppendLine();

        var collisions = new List<string>();
        if (_controller.collisions.left) collisions.Add("left");
        if (_controller.collisions.above) collisions.Add("above");
        if (_controller.collisions.right) collisions.Add("right");
        if (_controller.collisions.below) collisions.Add("below");
        if (collisions.Count == 0) collisions.Add("none");

        debugInfo.AppendFormat("collisions: {0}", string.Join(", ", collisions.ToArray()));
        debugInfo.AppendLine();

        debugInfo.AppendFormat("climbing slope: {0}", _controller.collisions.climbingSlope);
        debugInfo.AppendLine();

        debugInfo.AppendFormat("descending slope: {0}", _controller.collisions.descendingSlope);
        debugInfo.AppendLine();

        debugInfo.AppendFormat("sliding slope: {0}", _controller.collisions.slidingDownMaxSlope);
        debugInfo.AppendLine();

        debugInfo.AppendFormat("slope angle: {0}", _controller.collisions.slopeAngle);
        debugInfo.AppendLine();

        debugInfo.AppendFormat("slope normal: {0}", _controller.collisions.slopeNormal);
        debugInfo.AppendLine();

        debugInfo.AppendFormat("falling through: {0}", _controller.collisions.fallingThroughPlatform);
        debugInfo.AppendLine();

        DebugEx.ClearStatic();
        DebugEx.LogStatic(debugInfo.ToString());
    }

    public int Value
    {
        get { return _value; }
    }

    public bool IsDead
    {
        get { return _isDead; }
    }
}

public struct PlayerState
{
    public Vector2 position;
    public bool running;
    public bool jumping;
}