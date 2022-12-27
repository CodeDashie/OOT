using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class MovementJumpGravity : MonoBehaviour
{
    private PlayerActor _pA;

    const float PI = 3.14159265358979f;
    const float Rad2Deg = 180.0f / PI;
    const float Deg2Rad = PI / 180.0f;
    bool isStrafing = true;
    int MoveXAniParaId;
    int MoveZAniParaId;
    Vector2 CurrentAnimationBlendVector;
    Vector2 AnimationVelocity;

    public void SetValues(PlayerActor playerActor)
    {
        this._pA = playerActor;
        _pA.anim.speed = 3.5f;
        MoveXAniParaId = Animator.StringToHash("MoveX");
        MoveZAniParaId = Animator.StringToHash("MoveZ");
        
        _pA.HandRig.weight = 0.0f;
        _pA.AngleRig.weight = 0.0f;
        _pA.Shield.SetActive(false);
        _pA.isCrouch = false;
    }
    
    void FixedUpdate()
    {
        movement();
    }

    private void Update()
    {
        setJumpGravity();
    }

    void movement()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null || _pA.isCrouch)
            return;
        
        // desired velocity
        Vector2 v = new Vector2
        (
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        // animation speed based on magnitute of input axis
        float mag = new Vector2(v.x, v.y).magnitude;
        if (mag > 1.0f)
            mag = 1.0f;

        float tempX = v.x;
        v.x = v.x * Mathf.Sqrt(1 - 0.5f * (v.y * v.y));
        v.y = v.y * Mathf.Sqrt(1 - 0.5f * (tempX * tempX));


        // gravity and jump velocity
        if (!_pA.controller.isGrounded)
            _pA.fallVelocity += _pA.gravity;


        // facing, move and animation if ordered to
        if (!(v.x == 0.0f && v.y == 0.0f))
            fixFacingAngleToCamera(ref v, mag);

        // animation speed
        setAnimationSpeed(mag);

        // move to new position
        _pA.controller.Move(new Vector3(v.x, _pA.fallVelocity, v.y) * Time.deltaTime);
    }
    
    void Transition(bool b, string s)
    {
        _pA.anim.CrossFade(s, 0.35f, 0, 0.0f, 0.0f);
        isStrafing = !b;
    }

    void fixFacingAngleToCamera(ref Vector2 v, float mag)
    {
        // facing from desired direction with camera angle in mind
        float newFacing = (Rad2Deg * Mathf.Atan2(v.x, v.y)) +
                          _pA.camera.transform.eulerAngles.y;

        if (isStrafing)
            _pA.anim.SetFloat("Strafe", mag);

        // new walk position from newFacing
        v.x = (_pA.runSpeed * mag) * Mathf.Sin(newFacing * Deg2Rad);
        v.y = (_pA.runSpeed * mag) * Mathf.Cos(newFacing * Deg2Rad);
        // set new facing
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

    void setAnimationSpeed(float mag)
    {
        //if (isStrafing)
        _pA.anim.SetFloat(MoveZAniParaId, mag);
        //else
        //;
    }

    void setJumpGravity()
    {
        if (!_pA.isCrouch)
        {// state
            if (isStrafing)
            {
                if (!_pA.controller.isGrounded)
                {
                    // distance to ground
                    RaycastHit hit;
                    CharacterController charContr = _pA.controller;
                    Vector3 p1 = (transform.position + new Vector3(0, 1, 0)) + charContr.center + Vector3.up * -charContr.height * 0.4F;
                    Vector3 p2 = p1 + Vector3.up * charContr.height;

                    // Cast character controller shape 10 meters forward to see if it is about to hit anything.
                    if (Physics.CapsuleCast(p1, p2, charContr.radius, new Vector3(0, -1, 0), out hit, 10))
                        if (hit.distance < 3.0f)
                            return;

                    Transition(true, "Air");
                    return;
                }

                // jump
                if (isStrafing && !_pA.isHoldingObject)
                {
                    if (Input.GetButton("Jump"))
                    {
                        Transition(true, "Air");
                        _pA.fallVelocity = _pA.jumpVelocity;
                    }
                }
            }

            // landed
            else if (_pA.controller.isGrounded)
            {
                _pA.fallVelocity = _pA.groundedGravity;
                Transition(false, "Strafe");
            }
            else if (Input.GetButtonUp("Crouch"))
                _pA.anim.CrossFade("Air", 0.0f, 0, 0.0f, 0.0f);
        }
        // shield stuff
        if (Input.GetButtonDown("Shield") || Input.GetButtonDown("Crouch"))
        {
            if (!(Input.GetButton("Shield") && Input.GetButton("Crouch")))
            {
                _pA.Shield.SetActive(true);
                _pA.HandRig.weight = 1.0f;
                _pA.AngleRig.weight = 1.0f;
            }
            if (Input.GetButtonDown("Crouch"))
            {
                _pA.anim.CrossFade("Crouch", 0.0f, 0, 0.0f, 0.0f);
                _pA.isCrouch = true;
            }
        }
        else if (Input.GetButtonUp("Shield") || Input.GetButtonUp("Crouch"))
        {
            if (!Input.GetButton("Shield") && !Input.GetButton("Crouch"))
            {
                _pA.HandRig.weight = 0.0f;
                _pA.AngleRig.weight = 0.0f;
                _pA.Shield.SetActive(false);
            }
            if (Input.GetButtonUp("Crouch"))
            {
                _pA.anim.CrossFade("Strafe", 0.0f, 0, 0.0f, 0.0f);
                _pA.isCrouch = false;
            }
        }
    }
}
