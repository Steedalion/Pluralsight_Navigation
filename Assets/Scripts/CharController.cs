using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Coroutines;
using System.Linq;

public partial class CharController
	: MonoBehaviour
{
	[SerializeField]
	AnimationClip _ForwardClip;

	[SerializeField]
	float _StoppingDistance = 0.35f;

	[SerializeField]
	float _StoppingTransitionDuration = 0.25f;

	[SerializeField]
	float _RotationSpeed = 360.0f;

	[SerializeField]
	float _SnapAngleThreshold = 30.0f; // Degrees

	// Cached values
	Animator _Animator;
	int _ForwardId;

	// Computed values
	const float _MinMoveDistSqr = 0.0001f;
	bool _ApplyRootMotion = true;

	// Check navmesh
	public bool ConstrainAnimationDeltaToNavmesh
	{
		get;
		set;
	}

	void Awake()
	{
		_Animator = GetComponent<Animator>();
		_ForwardId = Animator.StringToHash("Forward");
	}

	// Use this for initialization
	void Start () 
	{
	}

	void OnAnimatorMove()
	{
		if (_ApplyRootMotion)
		{
			Vector3 newPos = _Animator.rootPosition;
			if (ConstrainAnimationDeltaToNavmesh)
			{
				NavMeshHit hitInfo;
				if (NavMesh.Raycast(transform.position, _Animator.rootPosition, out hitInfo, NavMesh.AllAreas))
				{
					if (hitInfo.normal.sqrMagnitude < 0.0001f)
					{
						newPos = hitInfo.position;
						// Raycast didn't work so well, it didn't return a normal, so just use the navmesh sampleposition method
						if (NavMesh.SamplePosition(_Animator.rootPosition, out hitInfo, Globals.Instance.Settings.NavMeshDistance, NavMesh.AllAreas))
						{
							newPos = hitInfo.position;
						}
						// Else stop at the edge
					}
					else
					{
						// Project the remaining delta Vector on the normal, this will have the effect of
						// making the movement slide on the edge of the navmesh
						Vector3 remainingDelta = newPos - hitInfo.position;
						remainingDelta.y = 0.0f;

						// Compute an edge vector, it doesn't matter which direction it points
						Vector3 edge = new Vector3(-hitInfo.normal.z, 0.0f, hitInfo.normal.x);

						newPos = hitInfo.position + Vector3.Project(remainingDelta, edge);
						if (NavMesh.Raycast(transform.position, newPos, out hitInfo, NavMesh.AllAreas))
						{
							// If we're still hitting the navmesh, then just clamp!
							newPos = hitInfo.position;
						}
						else
						{
							// No matter what, snap to the navmesh
							newPos.y = hitInfo.position.y;
						}
					}
				}
				else
				{
					// No matter what, snap to the navmesh
					newPos.y = hitInfo.position.y;
				}
			}
			transform.position = newPos;
			transform.rotation = _Animator.rootRotation;
		}
	}

    /// Everything from here down was added by DanoKablamo
    public bool Grounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.5f, -Vector3.up, distToGround + 0.3f);
    }
    public AudioSource playerSfxSource;
    public AudioClip[] woodSteps;
    public AudioClip[] hardSteps;
    public AudioClip[] grassSteps;
    RaycastHit hitInfo;
    float distToGround = 1f;

    public void Footsteps()
    {
        if (Grounded())
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, -Vector3.up, out hitInfo, distToGround + 0.7f))
            {
                int r = Random.Range(0, woodSteps.Length);
                switch (hitInfo.transform.GetComponent<Collider>().tag)
                {
                    case "HardFloor":
                       // Debug.Log("Hard Sound");

                        playerSfxSource.PlayOneShot(hardSteps[r]);
                        break;
                    case "WoodFloor":
                       // Debug.Log("Wood Sound");

                        playerSfxSource.PlayOneShot(woodSteps[r]);
                        break;
                    case "GrassFloor":
                      //  Debug.Log("Grass Sound");

                        playerSfxSource.PlayOneShot(grassSteps[r]);
                        break;
                    default:
                      //  Debug.Log("Default Sound");

                        playerSfxSource.PlayOneShot(hardSteps[r]);
                        break;
                }
            }
        }
    }
    ///End added by Dano Kablamo
}
