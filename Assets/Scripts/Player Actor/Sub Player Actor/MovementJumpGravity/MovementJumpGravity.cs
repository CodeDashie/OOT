using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementJumpGravity : MonoBehaviour
{
    private PlayerActor _pA;

    const float PI = 3.14159265358979f;
    const float Rad2Deg = 180.0f / PI;
    const float Deg2Rad = PI / 180.0f;

    public void SetValues(PlayerActor playerActor)
    {
        this._pA = playerActor;
    }

    void ResetAllTriggers()
    {
        foreach (AnimatorControllerParameter parameter in _pA.anim.parameters)
            _pA.anim.ResetTrigger(parameter.name);
    }

    void FixedUpdate()
    {
        movement();
    }

    void movement()
    {
        if (_pA.state == PlayerActor.State.WALKING)
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null)
            {
                return;
            }
            // desired velocity
            Vector2 v = new Vector2
            (
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            float mag = new Vector2(v.x, v.y).magnitude;
            if (mag > 1.0f)
                mag = 1.0f;

            float tempX = v.x;
            v.x = v.x * Mathf.Sqrt(1 - 0.5f * (v.y * v.y));
            v.y = v.y * Mathf.Sqrt(1 - 0.5f * (tempX * tempX));
            
            
            // facing, move and animation if ordered to
            if (!(v.x == 0.0f && v.y == 0.0f))
                fixFacingAngleToCamera(ref v, mag);
            
            // stop walk animation
            else
                setIdleAnimation();

            // gravity and jump
            setJumpGravity();

            // move to new position
            _pA.controller.Move(new Vector3(v.x, _pA.fallVelocity, v.y) * Time.deltaTime);
        }
    }

    void setIdleAnimation()
    {
        if (_pA.isWalking && _pA.controller.isGrounded)
        {
            _pA.isWalking = false;
            _pA.anim.ResetTrigger("isWalking");
            _pA.anim.SetTrigger("isIdling");
        }
    }

    void fixFacingAngleToCamera(ref Vector2 v, float mag)
    {
        // facing from desired direction with camera angle in mind
        float newFacing = (Rad2Deg * Mathf.Atan2(v.x, v.y)) +
                          _pA.camera.transform.eulerAngles.y;
        
        // new walk position from newFacing
        if (Input.GetButton("Sprint") && !_pA.isHoldingObject)
        {
            v.x = (_pA.runSpeed * mag) * Mathf.Sin(newFacing * Deg2Rad);
            v.y = (_pA.runSpeed * mag) * Mathf.Cos(newFacing * Deg2Rad);
            _pA.anim.speed = mag * 2.5f;
        }
        else
        {
            v.x = (_pA.walkSpeed * mag) * Mathf.Sin(newFacing * Deg2Rad);
            v.y = (_pA.walkSpeed * mag) * Mathf.Cos(newFacing * Deg2Rad);
            _pA.anim.speed = mag * 2.5f;
        }
        // run walk animation
        if (!_pA.isWalking && _pA.controller.isGrounded)
        {
            _pA.isWalking = true;
            _pA.anim.ResetTrigger("isIdling");
            _pA.anim.SetTrigger("isWalking");
        }
        setFacing(newFacing);
    }

    void setFacing(float newFacing)
    {
        float curFacing = _pA.transform.eulerAngles.y;

        if (newFacing > 360.0f)
            newFacing -= 360.0f;

        else if (newFacing < 0.0f)
            newFacing += 360.0f;

        // is there a difference between current frame facing and next frame ordered facing?
        if (curFacing != newFacing)
        {
            float velocity = _pA.turnSpeed * Time.deltaTime;

            // check if velocity is less than the difference
            if ((curFacing <= newFacing && curFacing + velocity >= newFacing) ||
                (curFacing >= newFacing && curFacing - velocity <= newFacing))
                curFacing = newFacing;
            
            else if (newFacing < 180)
            {
                if (curFacing > newFacing && curFacing < newFacing + 180)
                    curFacing -= velocity;
                else
                    curFacing += velocity;
            }
            else
            {
                if ((curFacing > newFacing && curFacing < newFacing + 180) || (curFacing + 360 > newFacing && curFacing < newFacing - 180))
                    curFacing -= velocity;
                else
                    curFacing += velocity;
            }

            if ((int)_pA.transform.eulerAngles.y != (int)curFacing)
                _pA.transform.rotation = Quaternion.Euler(_pA.transform.eulerAngles.x, curFacing, _pA.transform.eulerAngles.z);
        }
    }

    void setJumpGravity()
    {
        // jump
        if (_pA.controller.isGrounded && Input.GetButton("Jump") && !_pA.isHoldingObject)
        {
            _pA.anim.SetTrigger("isJumping");
            _pA.isJumping = true;
            _pA.fallVelocity = _pA.jumpVelocity;
            Debug.Log("jump");
        }
        // gravity
        else if (!_pA.controller.isGrounded)
        {
            ResetAllTriggers();
            if (!_pA.isHoldingObject && _pA.fallVelocity < -10.0f && !_pA.anim.GetCurrentAnimatorStateInfo(0).IsName("Falling"))
                _pA.anim.SetTrigger("isFalling");
            _pA.fallVelocity += _pA.gravity;
        }
        // landed
        else
        {
            _pA.anim.ResetTrigger("isJumping");
            _pA.anim.ResetTrigger("isFalling");
            _pA.fallVelocity = _pA.groundedGravity;
            // landed, so end jump animation if it's still going

            if (_pA.isJumping)
            {
                _pA.isJumping = false;
                if (_pA.isWalking)
                    _pA.anim.SetTrigger("isWalking");
                else if (!_pA.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                        _pA.anim.SetTrigger("isIdling");
            }
            else if(_pA.anim.GetCurrentAnimatorStateInfo(0).IsName("Falling"))
            {
                if (_pA.isWalking)
                    _pA.anim.SetTrigger("isWalking");
                else if (!_pA.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                        _pA.anim.SetTrigger("isIdling");
            }
        }
        
        // at end of animation
        if (_pA.isJumping && (_pA.anim.GetCurrentAnimatorStateInfo(0).IsName("Jump") || _pA.anim.GetCurrentAnimatorStateInfo(0).IsName("Falling")) &&
            _pA.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            _pA.isJumping = false;
            if (_pA.isWalking)
                ;// _pA.anim.SetTrigger("isWalking");
            else
                ;// _pA.anim.SetTrigger("isIdling");
            
        }
    }
}
