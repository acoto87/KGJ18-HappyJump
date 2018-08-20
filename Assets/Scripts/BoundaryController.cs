using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoundaryController : MonoBehaviour
{
    public GameObject boundary;

    private BoxCollider2D _collider;
    private Transform _player;
    
    void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        var bounds = _collider.bounds;
        boundary.SetActive(
            _player.position.x >= bounds.min.x && _player.position.x <= bounds.max.x &&
            _player.position.y >= bounds.min.y && _player.position.y <= bounds.max.y);
    }
}
