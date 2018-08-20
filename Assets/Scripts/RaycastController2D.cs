using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController2D : MonoBehaviour
{
    public const float skinWidth = 0.015f;

    public float distanceBetweenRays = 0.25f;
    public LayerMask collisionMask;

    protected BoxCollider2D _collider;
    internal RaycastOrigins _raycastOrigins;

    protected int _horizontaRayCount;
    protected int _verticalRayCount;
    protected float _horizontaRaySpacing;
    protected float _verticalRaySpacing;

    protected void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    protected void CalculateRaySpacing()
    {
        var bounds = _collider.bounds;
        bounds.Expand(-2 * skinWidth);

        var boundsWidth = bounds.size.x;
        var boundsHeight = bounds.size.y;

        _horizontaRayCount = Mathf.RoundToInt(boundsHeight / distanceBetweenRays);
        _verticalRayCount = Mathf.RoundToInt(boundsWidth / distanceBetweenRays);

        _horizontaRaySpacing = boundsHeight / (_horizontaRayCount - 1);
        _verticalRaySpacing = boundsWidth / (_verticalRayCount - 1);
    }

    protected void UpdateRaycastOrigins()
    {
        var bounds = _collider.bounds;
        bounds.Expand(-2 * skinWidth);

        _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
