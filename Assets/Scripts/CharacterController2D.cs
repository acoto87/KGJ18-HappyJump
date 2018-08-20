using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : RaycastController2D
{
    public CollisionInfo collisions;

    public float maxSlopeAngle = 60.0f;

    internal float _facingDirection;
    internal Vector2 _playerInput;

    public void Move(Vector2 deltaMovement, bool standingOnPlatform = false)
    {
        Move(deltaMovement, new Vector2(_facingDirection, 0.0f), standingOnPlatform);
    }

    public void Move(Vector2 deltaMovement, Vector2 playerInput, bool standingOnPlatform = false)
    {
        _facingDirection = 0.0f;
        _playerInput = playerInput;

        CalculateRaySpacing();
        UpdateRaycastOrigins();

        collisions.Reset();

        if (deltaMovement.x != 0)
        {
            _facingDirection = Mathf.Sign(deltaMovement.x);
        }

        if (deltaMovement.y < 0)
        {
            DescendSlope(ref deltaMovement);
        }
        
        // the DescendSlope method potentially could modify the x component of the deltaMovement,
        // so recalculate _facingDirection
        if (deltaMovement.x != 0)
        {
            _facingDirection = Mathf.Sign(deltaMovement.x);
        }

        HorizontalCollisions(ref deltaMovement);

        if (deltaMovement.y != 0)
        {
            VerticalCollisions(ref deltaMovement);
        }

        transform.Translate(deltaMovement);

        if (standingOnPlatform)
        {
            collisions.below = true;
        }
    }

    private void HorizontalCollisions(ref Vector2 deltaMovement)
    {
        var rayLength = Mathf.Abs(deltaMovement.x) + skinWidth;

        for (int i = 0; i < _horizontaRayCount; i++)
        {
            var rayOrigin = _facingDirection < 0 ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (_horizontaRaySpacing * i);

            Debug.DrawRay(rayOrigin, Vector2.right * _facingDirection * rayLength, Color.red);

            var hit = Physics2D.Raycast(rayOrigin, Vector2.right * _facingDirection, rayLength, collisionMask);
            if (hit)
            {
                if (hit.distance == 0)
                {
                    continue;
                }

                var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                
                // check for the slope collision only in the bottom-most ray
                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    ClimbSlope(ref deltaMovement, slopeAngle, hit.normal);
                }

                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    deltaMovement.x = (hit.distance - skinWidth) * _facingDirection;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        deltaMovement.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(deltaMovement.x);
                    }

                    collisions.left = (_facingDirection < 0);
                    collisions.right = (_facingDirection > 0);
                }
            }
        }
    }

    private void VerticalCollisions(ref Vector2 deltaMovement)
    {
        var directionY = Mathf.Sign(deltaMovement.y);
        var rayLength = Mathf.Abs(deltaMovement.y) + skinWidth;

        for (int i = 0; i < _verticalRayCount; i++)
        {
            var rayOrigin = directionY < 0 ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (_verticalRaySpacing * i + deltaMovement.x);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            var hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            if (hit)
            {
                if (hit.transform.CompareTag("Through"))
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }

                    if (collisions.fallingThroughPlatform)
                    {
                        continue;
                    }

                    if (_playerInput.y < 0)
                    {
                        collisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", 0.5f);
                        continue;
                    }
                }

                deltaMovement.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    deltaMovement.x = deltaMovement.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(deltaMovement.x);
                }

                collisions.below = directionY < 0;
                collisions.above = directionY > 0;
            }
        }
    }

    private void ClimbSlope(ref Vector2 deltaMovement, float slopeAngle, Vector2 slopeNormal)
    {
        var moveDistance = Mathf.Abs(deltaMovement.x);
        var climbMovementY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (deltaMovement.y <= climbMovementY)
        {
            deltaMovement.y = climbMovementY;
            deltaMovement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(deltaMovement.x);
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = slopeNormal;
            collisions.below = true;
        }
    }

    private void DescendSlope(ref Vector2 deltaMovement)
    {
        var maxSlopeHitLeft = Physics2D.Raycast(_raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(deltaMovement.y) + skinWidth, collisionMask);
        var maxSlopeHitRight = Physics2D.Raycast(_raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(deltaMovement.y) + skinWidth, collisionMask);

        // only if exactly one of the corners collide with a slope, we call SlideDownMaxSlope.
        // this prevent a glitch when the player is starting to descend a slope, but part of the body of the character is still in flat surface.
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            if (maxSlopeHitLeft)
            {
                SlideDownMaxSlope(maxSlopeHitLeft, ref deltaMovement);
            }

            if (maxSlopeHitRight)
            {
                SlideDownMaxSlope(maxSlopeHitRight, ref deltaMovement);
            }
        }

        if (!collisions.slidingDownMaxSlope)
        {
            var rayOrigin = _facingDirection < 0 ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;

            var hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);
            if (hit)
            {
                var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    // if the slope is in the direction of the descend
                    if (Mathf.Sign(hit.normal.x) == _facingDirection)
                    {
                        // recalculate x and y components treating the deltaMovement.x as the total move distance down the slope
                        var moveDistance = Mathf.Abs(deltaMovement.x);
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * moveDistance)
                        {
                            deltaMovement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * _facingDirection;
                            deltaMovement.y -= Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

                            collisions.descendingSlope = true;
                            collisions.slopeAngle = slopeAngle;
                            collisions.slopeNormal = hit.normal;
                            collisions.below = true;
                        }
                    }
                }
            }
        }
    }

    private void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 deltaMovement)
    {
        var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        if (slopeAngle > maxSlopeAngle)
        {
            var distanceToSlope = Mathf.Abs(deltaMovement.y) - hit.distance;
            deltaMovement.x = Mathf.Sign(hit.normal.x) * Mathf.Abs(distanceToSlope) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = hit.normal;
            collisions.slidingDownMaxSlope = true;
        }
    }

    private void ResetFallingThroughPlatform()
    {
        collisions.fallingThroughPlatform = false;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;
        public float slopeAngle;
        public Vector2 slopeNormal;
        public bool fallingThroughPlatform;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = descendingSlope = false;
            slidingDownMaxSlope = false;
            slopeAngle = 0.0f;
            slopeNormal = Vector2.zero;
            fallingThroughPlatform = false;
        }
    }
}
