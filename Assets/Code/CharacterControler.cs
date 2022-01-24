using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControler : MonoBehaviour
{
    public enum CharacterPhysicsState
    {
        GROUNDED,
        JUMPING,
        POST_JUMP,
        FALLING,
    }

    public CharacterPhysicsState _State;

    public Rigidbody _FootWheel;
    public FootCollisionTracker _FootCollisionTracker;
    public Rigidbody _Body;

    public float _legSpinSpeed = 100;

    public float _MassOffset = -1;

    public float _JumpForce = 100;

    public float _JumpAcelForce = 10;

    public float _JumpBreakForce = 20;

    public float _JumpMaxSpeed = 20;

    public float _JumpGravity = 5;

    public float _FallGravity = 15;

    public float _StandGravity = 10;

    public float _JumpTime;

    public float _CoyoteTime;

    public float _CoyoteGravity;

    public Vector3 _GravityOffset = new Vector3(0,-5,0);
    public Vector3 _GravityDirection = new Vector3(0,-1,0);

    public string _Left = "a";
    public string _Right = "d";
    public string _Jump = "space";

    private Vector3 _StartCenterOfMass;

    private float _TimeSinceLastGroundContact;

    private bool _WasJumpPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        _StartCenterOfMass = _Body.centerOfMass;
        _FootWheel.maxAngularVelocity = _legSpinSpeed;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if(_FootCollisionTracker.state != FootCollisionTracker.FootCollisionState.GROUNDED)
        {

            //update time since last ground contact
            _TimeSinceLastGroundContact += Time.fixedDeltaTime;
        }
        else
        {
            _TimeSinceLastGroundContact = 0;
        }
        

        //change center of mass
        _Body.centerOfMass = new Vector3(_StartCenterOfMass.x, _StartCenterOfMass.y + _MassOffset, _StartCenterOfMass.z);

        UpdatePhysState();


        // do leg rotation
        //Quaternion amountToRotate = Quaternion.Euler(0, 0, _legSpinSpeed * Time.fixedDeltaTime);
        //
        //Quaternion currentRotation = _LegWheel.rotation;
        //
        //Quaternion newRotation = currentRotation * amountToRotate;
        //
        //_LegWheel.MoveRotation(newRotation);

        //_LegWheel.an


        //apply gravity 


    }

    void ApplyFootMovement(float speed)
    {
        //rotate the wheel
        float rotationDir = 0;

        if (Input.GetKey(_Left))
        {
            rotationDir += 1;
        }

        if (Input.GetKey(_Right))
        {
            rotationDir -= 1;
        }

        _FootWheel.angularVelocity = new Vector3(0, 0, speed * rotationDir);

    }

    void ApplyFlightForce(float accelForce, float breakForce)
    {
        Vector3 relativeMovement = _Body.velocity - _FootCollisionTracker._CollisionSurfaceVelocity;

        float horizontalMovement = Vector3.Dot(_Body.transform.right, relativeMovement);

        float leftForce = (horizontalMovement > -_JumpMaxSpeed) ? ((horizontalMovement >_JumpMaxSpeed) ? breakForce : accelForce) : 0;
        float rightForce = (horizontalMovement  < _JumpMaxSpeed) ? ((horizontalMovement < -_JumpMaxSpeed)? breakForce : accelForce) : 0;

        if (Input.GetKey(_Left))
        {
            _Body.AddForceAtPosition(_Body.transform.right * -leftForce,_Body.worldCenterOfMass, ForceMode.Force);
        }

        if (Input.GetKey(_Right))
        {
            _Body.AddForceAtPosition(_Body.transform.right * rightForce, _Body.worldCenterOfMass, ForceMode.Force);
        }

    }

    void ApplyBallancingGravity(float gravity)
    {
        Vector3 gravForcePos = _Body.position + (_Body.rotation * _GravityOffset);


        Debug.DrawLine(_Body.position, gravForcePos, Color.blue);

        _Body.AddForceAtPosition(_GravityDirection * gravity, gravForcePos, ForceMode.Force);
    }

    bool ApplyJumpForce(float jumpForce)
    {
        if (Input.GetKey(_Jump))
        {
            if (_WasJumpPressed == false)
            {
                _Body.AddForceAtPosition(_GravityDirection * _JumpForce, _Body.worldCenterOfMass);
                _WasJumpPressed = true;
                
            }

            return true;

        }
        else
        {
            _WasJumpPressed = false;
        }

        return false;
    }

    private void UpdatePhysState()
    {
        switch(_State)
        {
            case CharacterPhysicsState.GROUNDED:
                {
                    UpdateGroundedPhysState();
                    break;
                }

            case CharacterPhysicsState.FALLING:
                {
                    UpdateFallPhysState();
                    break;
                }

            case CharacterPhysicsState.JUMPING:
                {
                    UpdateJumpPhysState();
                    break;
                }

            case CharacterPhysicsState.POST_JUMP:
                {
                    UpdatePostJumpPhysState();
                    break;
                }

        }
    }

    private void UpdateGroundedPhysState()
    {
        ApplyFootMovement(_legSpinSpeed);
        ApplyBallancingGravity(_StandGravity);

        if (ApplyJumpForce(_JumpForce))
        {
            _State = CharacterPhysicsState.JUMPING;
        }
        else if(_TimeSinceLastGroundContact > 0)
        {
            _State = CharacterPhysicsState.FALLING;
        }
    }

    private void UpdateFallPhysState()
    {       
        ApplyFootMovement(_legSpinSpeed);
        ApplyFlightForce(_JumpAcelForce, _JumpBreakForce);

        if (_TimeSinceLastGroundContact < _CoyoteTime || _TimeSinceLastGroundContact == 0)
        {
           if(ApplyJumpForce(_JumpForce))
            {
                _State = CharacterPhysicsState.JUMPING;
            }
           else if(_TimeSinceLastGroundContact == 0)
            {
                _State = CharacterPhysicsState.GROUNDED;
            }

            ApplyBallancingGravity(_CoyoteGravity);
        }
        else
        {
            //give some coyote time
            ApplyBallancingGravity(_FallGravity);
        }
    }

    private void UpdateJumpPhysState()
    {
        ApplyBallancingGravity(_JumpGravity);
        ApplyFlightForce(_JumpAcelForce, _JumpBreakForce);

        if (!Input.GetKey(_Jump) || _TimeSinceLastGroundContact > _JumpTime)
        {
            _State = CharacterPhysicsState.POST_JUMP;
        }

        if (_TimeSinceLastGroundContact == 0)
        {
            _State = CharacterPhysicsState.GROUNDED;
        }

    }

    private void UpdatePostJumpPhysState()
    {
        ApplyBallancingGravity(_FallGravity);
        ApplyFlightForce(_JumpAcelForce, _JumpBreakForce);

        if (_TimeSinceLastGroundContact == 0)
        {
            _State = CharacterPhysicsState.GROUNDED;
        }
    }


}
