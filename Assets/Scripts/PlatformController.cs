using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController2D
{
    public float speed;
    public LayerMask passengerMask;
    public Vector2[] localWaypoints;
    public bool repeat;
    public bool cyclic;
    public float waitTime;
    [Range(0, 2)]
    public float easeAmount;

    private Vector3 _velocity;
    private List<PassengerMovement> _passengerMovements;
    private Vector3[] _globalWaypoints;
    private int _fromWaypointIndex;
    private float _percentBetweenWaypoints;
    private float _nextMoveTime;

    protected void Start()
    {
        _globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            _globalWaypoints[i] = (Vector3)localWaypoints[i] + transform.position;
        }
    }

    void Update()
    {
        _velocity = Vector3.zero;

        UpdateRaycastOrigins();
        CalculateRaySpacing();

        CalculatePlatformMovement();
        CalculatePassengersMovement();

        MovePassengers(true);
        transform.Translate(_velocity);
        MovePassengers(false);
    }

    private void CalculatePlatformMovement()
    {
        if (_globalWaypoints == null || _globalWaypoints.Length == 0)
        {
            return;
        }

        if (Time.time < _nextMoveTime)
        {
            return;
        }

        if (_percentBetweenWaypoints >= 1)
        {
            if (cyclic)
            {
                _fromWaypointIndex = (_fromWaypointIndex + 1) % _globalWaypoints.Length;
            }
            else
            {
                _fromWaypointIndex++;
            }

            _percentBetweenWaypoints = 0;
        }

        var toWaypointIndex = _fromWaypointIndex + 1;
        if (toWaypointIndex >= _globalWaypoints.Length)
        {
            if (!repeat)
            {
                return;
            }

            if (cyclic)
            {
                toWaypointIndex %= _globalWaypoints.Length;
            }
            else
            {
                Array.Reverse(_globalWaypoints);
                _fromWaypointIndex = 0;
                toWaypointIndex = 1;
            }
        }

        var distanceBetweenWaypoints = Vector3.Distance(_globalWaypoints[_fromWaypointIndex], _globalWaypoints[toWaypointIndex]);
        _percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        _percentBetweenWaypoints = Mathf.Clamp01(_percentBetweenWaypoints);
        var easedPercentBetweenWaypoints = Ease(_percentBetweenWaypoints);

        var newPosition = Vector3.Lerp(_globalWaypoints[_fromWaypointIndex], _globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);
        _velocity = newPosition - transform.position;

        if (_percentBetweenWaypoints >= 1)
        {
            _nextMoveTime = Time.time + waitTime;
        }
    }

    private float Ease(float x)
    {
        var a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    private void MovePassengers(bool beforeMovePlatform)
    {
        for (int i = 0; i < _passengerMovements.Count; i++)
        {
            var passenger = _passengerMovements[i];
            if (passenger.moveBeforePlatform == beforeMovePlatform)
            {
                var controller2D = passenger.transform.GetComponent<CharacterController2D>();
                controller2D.Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    private void CalculatePassengersMovement()
    {
        _passengerMovements = new List<PassengerMovement>();

        var movedPassangers = new HashSet<Transform>();

        var directionX = Mathf.Sign(_velocity.x);
        var directionY = Mathf.Sign(_velocity.y);

        // Vertical moving platform
        if (_velocity.y != 0)
        {
            var rayLength = Mathf.Abs(_velocity.y) + skinWidth;
            for (int i = 0; i < _verticalRayCount; i++)
            {
                var rayOrigin = directionY < 0 ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
                rayOrigin += Vector2.right * _verticalRaySpacing * i;

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

                var hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                if (hit && hit.distance > 0)
                {
                    if (!movedPassangers.Contains(hit.transform))
                    {
                        var pushX = directionY > 0 ? _velocity.x : 0.0f;
                        var pushY = _velocity.y - (hit.distance - skinWidth) * directionY;

                        _passengerMovements.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), directionY > 0, true));

                        movedPassangers.Add(hit.transform);
                    }
                }
            }
        }

        // Horizontally moving platform
        if (_velocity.x != 0)
        {
            var rayLength = Mathf.Abs(_velocity.x) + skinWidth;
            for (int i = 0; i < _horizontaRayCount; i++)
            {
                var rayOrigin = directionX < 0 ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * _horizontaRaySpacing * i;

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                if (hit && hit.distance > 0)
                {
                    if (!movedPassangers.Contains(hit.transform))
                    {
                        var pushX = _velocity.x - (hit.distance - skinWidth) * directionX;
                        var pushY = -skinWidth;

                        _passengerMovements.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), false, true));

                        movedPassangers.Add(hit.transform);
                    }
                }
            }
        }

        // Passenger is on top of horizontally or downward moving platform
        if (_velocity.y < 0 || _velocity.y == 0 && _velocity.x != 0)
        {
            var rayLength = 2 * skinWidth;
            for (int i = 0; i < _verticalRayCount; i++)
            {
                var rayOrigin = _raycastOrigins.topLeft;
                rayOrigin += Vector2.right * _verticalRaySpacing * i;

                Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.red);

                var hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
                if (hit && hit.distance > 0)
                {
                    if (!movedPassangers.Contains(hit.transform))
                    {
                        var pushX = _velocity.x;
                        var pushY = _velocity.y;

                        _passengerMovements.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), true, false));

                        movedPassangers.Add(hit.transform);
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;

            var size = 0.3f;
            for (int i = 0; i < localWaypoints.Length; i++)
            {
                var globalWaypointPosition = Application.isPlaying ? _globalWaypoints[i] : ((Vector3)localWaypoints[i] + transform.position);
                Gizmos.DrawLine(globalWaypointPosition - Vector3.up * size, globalWaypointPosition + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPosition - Vector3.left * size, globalWaypointPosition + Vector3.left * size);
            }
        }
    }

    struct PassengerMovement
    {
        public Transform transform;
        public Vector2 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform transform, Vector2 velocity, bool standingOnPlatform, bool moveBeforePlatform)
        {
            this.transform = transform;
            this.velocity = velocity;
            this.standingOnPlatform = standingOnPlatform;
            this.moveBeforePlatform = moveBeforePlatform;
        }
    }
}
