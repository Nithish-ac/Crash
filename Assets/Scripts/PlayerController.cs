using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.EventSystems;
using Fusion.Addons.Physics;
using UnityEngine.Windows;
using Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelAudio;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private NetworkRigidbody2D _rb;
    [SerializeField] private Sprite[] _sprites;

    private SpriteRenderer _player;
    private Vector2 _direction;
    private List<GameObject> connectedPlayers = new List<GameObject>();

    private void Start()
    {
        _player = GetComponent<SpriteRenderer>();
    }

    // Update player movement based on input
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            float moveX = UnityEngine.Input.GetAxis("Horizontal");
            float moveY = UnityEngine.Input.GetAxis("Vertical");

            _direction = new Vector2(moveX, moveY).normalized;

            _rb.Rigidbody.velocity = _direction * _speed;
        }

    }

    // Handle player collision for audio communication
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!connectedPlayers.Contains(collision.gameObject))
            {
                connectedPlayers.Add(collision.gameObject);
                JoinChannelAudio._instance.JoinChannel();
            }
            _player.sprite = _sprites[1];
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (connectedPlayers.Contains(collision.gameObject))
            {
                connectedPlayers.Remove(collision.gameObject);
                if (connectedPlayers.Count == 0)
                {
                    JoinChannelAudio._instance.LeaveChannel();
                    _player.sprite = _sprites[0];
                }
            }
        }
    }
}


