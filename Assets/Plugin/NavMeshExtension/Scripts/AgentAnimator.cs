/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using UnityEngine.AI;

namespace NavMeshExtension
{
    /// <summary>
    /// Mecanim motion animator for movement scripts.
    /// Passes movement direction to the Mecanim controller.
    /// This script is an adaption from Unity's official animation page.
    /// <summary>
    //https://docs.unity3d.com/Manual/nav-CouplingAnimationAndNavigation.html
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class AgentAnimator : MonoBehaviour
    {
        //movement script references
        private NavMeshAgent agent;
        //Mecanim animator reference
        private Animator animator;
        //calculated agent movement positions
        private Vector2 smoothPos = Vector2.zero;
        private Vector2 velocity = Vector2.zero;


        //getting component references
        void Start()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            agent.updatePosition = false;
        }


        void Update()
        {
            //map world delta position to local space
            Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
            float dx = Vector3.Dot(transform.right, worldDeltaPosition);
            float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
            Vector2 deltaPosition = new Vector2(dx, dy);

            //low-pass filter the delta movement
            float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
            smoothPos = Vector2.Lerp(smoothPos, deltaPosition, smooth);

            //update velocity if delta time is safe
            if (Time.deltaTime > 1e-5f)
                velocity = smoothPos / Time.deltaTime;

            //did the agent move?
            bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

            //update animation parameters
            animator.SetBool("moving", shouldMove);
            animator.SetFloat("vX", velocity.x);
            animator.SetFloat("vY", velocity.y);

            //pull agent towards character to stay in sync
            if (worldDeltaPosition.magnitude > agent.radius)
                agent.nextPosition = transform.position + 0.9f * worldDeltaPosition;
        }


        //method override for root motion on the animator
        void OnAnimatorMove()
        {
            /* OLD VERSION
            //init variables
            float speed = 0f;
            float angle = 0f;

            //calculate variables based on movement script:
            //get the agent's speed and calculate the rotation difference to the last frame
            speed = nAgent.velocity.magnitude;
            Vector3 velocity = Quaternion.Inverse(transform.rotation) * nAgent.desiredVelocity;
            angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            //push variables to the animator with some optional damping
            animator.SetFloat("Speed", speed);
            animator.SetFloat("Direction", angle, 0.15f, Time.deltaTime);
            */

            //update position based on animation movement using navigation surface height
            Vector3 position = animator.rootPosition;
            position.y = agent.nextPosition.y;
            transform.position = position;
        }
    }
}