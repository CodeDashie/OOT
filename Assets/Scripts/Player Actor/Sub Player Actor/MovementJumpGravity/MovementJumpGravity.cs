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
    bool isStrafing = true;
    int MoveXAniParaId;
    int MoveZAniParaId;
    Vector2 CurrentAnimationBlendVector;
    Vector2 AnimationVelocity;
    GameObject Shield;

    GameObject GetChildWithName(GameObject obj, string name)
    {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null)
        {
            return childTrans.gameObject;
        }
        else
        {
            return null;
        }
    }
    public void SetValues(PlayerActor playerActor)
    {
        this._pA = playerActor;
        _pA.anim.speed = 3.5f;
        MoveXAniParaId = Animator.StringToHash("MoveX");
        MoveZAniParaId = Animator.StringToHash("MoveZ");
        Shield = GetChildWithName(this.gameObject, "Shield");
    }
    
    void FixedUpdate()
    {
        movement();
    }

    void movement()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
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


        // gravity and jump
        setJumpGravity();

        // facing, move and animation if ordered to
        if (!(v.x == 0.0f && v.y == 0.0f))
            fixFacingAngleToCamera(ref v, mag);

        // animation speed
        setAnimationSpeed(mag);

        // move to new position
        _pA.controller.Move(new Vector3(v.x, _pA.fallVelocity, v.y) * Time.deltaTime);
    }
    
    void SetState(bool b)
    {
        _pA.anim.SetBool("IsGround", !b);
        _pA.anim.SetBool("IsAir", b);
        isStrafing = !b;
    }

    void Transition(bool b, string s)
    {
        _pA.anim.CrossFade(s, 0.35f);
        isStrafing = !b;
    }

    void setJumpGravity()
    {
        // gravity
        if (!_pA.controller.isGrounded)
            _pA.fallVelocity += _pA.gravity;

        // state
        if (isStrafing)
        {
            if (!_pA.controller.isGrounded)
            {
                // distance to ground
                RaycastHit hit;
                CharacterController charContr = _pA.controller;
                Vector3 p1 = transform.position + charContr.center + Vector3.up * -charContr.height * 0.4F;
                Vector3 p2 = p1 + Vector3.up * charContr.height;
                //float distanceToObstacle = 0;

                // Cast character controller shape 10 meters forward to see if it is about to hit anything.
                if (Physics.CapsuleCast(p1, p2, charContr.radius, new Vector3(0, -1, 0), out hit, 10)
                    && hit.distance < 2.0f)
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
                else if (Input.GetButton("Shield"))
                {

                }
            }
        }
        // landed
        else if (_pA.controller.isGrounded)
        {
            Debug.Log("isGrounded");
            _pA.fallVelocity = _pA.groundedGravity;
            Transition(false, "Strafe");
        }
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
}
