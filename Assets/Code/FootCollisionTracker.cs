using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This keeps track of the agents feet and changes state if enters collision
public class FootCollisionTracker : MonoBehaviour
{
    public enum FootCollisionState
    {
        GROUNDED,
        SLIDE,
        NO_GROUND
    }

    public FootCollisionState state;

    public float _FallAngle = 0.9f;
    public float _SlideAngle = 0.75f;
    public Vector3 _up = Vector3.up;
    public Vector3 _CollisionSurfaceVelocity = Vector3.zero;


    private float _MaxCollisionAllignment;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
        if(_MaxCollisionAllignment < (1.0f - _FallAngle))
        {
            state = FootCollisionState.NO_GROUND;
        }
        else if(_MaxCollisionAllignment < (1.0f - _SlideAngle))
        {
            state = FootCollisionState.SLIDE;
        }
        else
        {
            state = FootCollisionState.GROUNDED;
        }

        _MaxCollisionAllignment = 0;
    }
    private void OnCollisionStay(Collision collision)
    {
        //get the flattests surface the character is colliding with
        for(int i = 0;  i < collision.contactCount; i++ )
        {
            float collisionAlignment = Vector3.Dot(collision.contacts[i].normal, _up);
            if (collisionAlignment > _MaxCollisionAllignment)
            {
                _MaxCollisionAllignment = collisionAlignment;
                
                if(collision.rigidbody != null)
                {
                    _CollisionSurfaceVelocity = collision.rigidbody.GetPointVelocity(transform.position);
                }
                else
                {
                    _CollisionSurfaceVelocity = Vector3.zero;
                }
            }
        }        
    }
}
