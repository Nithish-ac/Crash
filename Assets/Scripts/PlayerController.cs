using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.EventSystems;
using Fusion.Addons.Physics;

public class PlayerController : NetworkBehaviour
{
    private NetworkRigidbody2D _rb;
    [SerializeField] float _speed = 10f;
    void Start()
    {
        _rb = GetComponent<NetworkRigidbody2D>();
    }

    void Update()
    {
        if (Object.HasInputAuthority)
        {
            Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            _rb.Rigidbody.velocity = move * _speed;
        }
    }

    //public override void FixedUpdateNetwork()
    //{
    //    if (GetInput<InputData>(out var input))
    //    {
    //    if (input.GetButton(InputButton.LEFT))
    //        {
    //            //Reset x velocity if start moving in oposite direction.
    //            if (_rb.Rigidbody.velocity.x > 0)
    //            {
    //                _rb.Rigidbody.velocity *= Vector2.up;
    //            }
    //            _rb.Rigidbody.AddForce(Vector2.left * _speed * Runner.DeltaTime, ForceMode2D.Force);
    //        }
    //        else if (input.GetButton(InputButton.RIGHT) && _behaviour.InputsAllowed)
    //        {
    //            //Reset x velocity if start moving in oposite direction.
    //            if (_rb.Rigidbody.velocity.x < 0 && IsGrounded)
    //            {
    //                _rb.Rigidbody.velocity *= Vector2.up;
    //            }
    //            _rb.Rigidbody.AddForce(Vector2.right * _speed * Runner.DeltaTime, ForceMode2D.Force);
    //        }
    //    }
    //}
}
