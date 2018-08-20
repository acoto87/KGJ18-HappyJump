using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    public CharacterController2D target;
    public Vector2 focusAreaSize;

    public float verticalOffset = 1.0f;
    public float verticalSmoothTime = 0.1f;
    public float lookAheadDistanceX = 2.0f;
    public float lookAheadSmoothTimeX = 0.5f;

    private FocusArea _focusArea;

    private float currentLookAheadX;
    private float targetLookAheadX;
    private float lookAheadDirX;
    private float lookAheadSmoothVelocityX;
    private float verticalSmoothVelocity;
    private bool lookAheadStopped;

    private Vector2 _cameraSize;

    void Start()
    {
        var collider = target.GetComponent<BoxCollider2D>();
        _focusArea = new FocusArea(collider.bounds, focusAreaSize);

        _cameraSize.y = Camera.main.orthographicSize * 2;
        _cameraSize.x = _cameraSize.y * 16.0f / 9.0f;
    }

    void LateUpdate()
    {
        var playerCollider = target.GetComponent<BoxCollider2D>();
        _focusArea.Update(playerCollider.bounds);

        if (_focusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(_focusArea.velocity.x);
            if (target._playerInput.x != 0 && Mathf.Sign(target._playerInput.x) == Mathf.Sign(_focusArea.velocity.x))
            {
                targetLookAheadX = lookAheadDirX * lookAheadDistanceX;
                lookAheadStopped = false;
            }
            else if (!lookAheadStopped)
            {
                targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistanceX - currentLookAheadX) * 0.25f;
                lookAheadStopped = true;
            }
        }

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref lookAheadSmoothVelocityX, lookAheadSmoothTimeX);

        var focusPosition = _focusArea.center + Vector2.up * verticalOffset;
        focusPosition.x += currentLookAheadX;
        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref verticalSmoothVelocity, verticalSmoothTime);

        var boundary = GameObject.FindGameObjectWithTag("Boundary");
        if (boundary)
        {
            var boundaryCollider = boundary.GetComponent<BoxCollider2D>();
            focusPosition.x = Mathf.Clamp(focusPosition.x, boundaryCollider.bounds.min.x + _cameraSize.x * 0.5f, boundaryCollider.bounds.max.x - _cameraSize.x * 0.5f);
            focusPosition.y = Mathf.Clamp(focusPosition.y, boundaryCollider.bounds.min.y + _cameraSize.y * 0.5f, boundaryCollider.bounds.max.y - _cameraSize.y * 0.5f);
        }

        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Gizmos.DrawCube(_focusArea.center, focusAreaSize);
        Gizmos.DrawSphere((Vector3)_focusArea.center + Vector3.right * lookAheadDistanceX * target._facingDirection, 0.2f);
    }

    struct FocusArea
    {
        public Vector2 center;
        public Vector2 velocity;

        public float left, right;
        public float top, bottom;

        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x * 0.5f;
            right = targetBounds.center.x + size.x * 0.5f;
            top = targetBounds.center.y + size.y * 0.5f;
            bottom = targetBounds.center.y - size.y * 0.5f;
            center = new Vector2((left + right) * 0.5f, (top + bottom) * 0.5f);
            velocity = Vector2.zero;
        }

        public void Update(Bounds targetBounds)
        {
            var shiftX = 0.0f;
            if (targetBounds.min.x < left)
            {
                shiftX = targetBounds.min.x - left;
            }
            else if (targetBounds.max.x > right)
            {
                shiftX = targetBounds.max.x - right;
            }

            var shiftY = 0.0f;
            if (targetBounds.min.y < bottom)
            {
                shiftY = targetBounds.min.y - bottom;
            }
            else if (targetBounds.max.y > top)
            {
                shiftY = targetBounds.max.y - top;
            }

            left += shiftX;
            right += shiftX;
            top += shiftY;
            bottom += shiftY;
            center = new Vector2((left + right) * 0.5f, (top + bottom) * 0.5f);
            velocity = new Vector2(shiftX, shiftY);
        }
    }
}
