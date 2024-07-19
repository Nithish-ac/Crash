using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private NetworkRigidbody2D _rb;
    [SerializeField] private Sprite[] _sprites;

    private int _playerID;
    private string _channelName;
    private string _token;
    private SpriteRenderer _player;
    private Vector2 _direction;
    private List<GameObject> neighbours = new List<GameObject>();

    private void Start()
    {
        _player = GetComponent<SpriteRenderer>();
        _playerID = SetPlayerID();
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
        if (collision.gameObject.tag == "Player" && !neighbours.Contains(collision.gameObject))
        {
            neighbours.Add(collision.gameObject);
            PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();
            AgoraManager.Instance.JoinChannel(this, otherPlayer);
            _player.sprite = _sprites[1];
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && neighbours.Contains(collision.gameObject))
        {
            neighbours.Remove(collision.gameObject);
            if (neighbours.Count == 0)
            {
                AgoraManager.Instance.LeaveChannel(this);
                _player.sprite = _sprites[0];
            }
        }
    }
    public string GetChannelName() { return _channelName; }
    public void SetChannelName(string name) { _channelName = name; }

    public string GetToken() { return _token; }
    public void SetToken(string newToken) { _token = newToken; }

    public int GetPlayerId() { return _playerID; }
    public static int SetPlayerID() => UnityEngine.Random.Range(10000, 99999);
    public void TriggerJoin(PlayerController _playerController) => AgoraManager.Instance.JoinChannel(this,_playerController);
}


