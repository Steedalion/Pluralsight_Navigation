using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CharacterDirectControl
	: MonoBehaviour
{
	[SerializeField]
	float _TurnSpeed = 360.0f;

	[SerializeField]
	float _MoveSpeed = 4.0f;

	[SerializeField]
	float _StartMovingThreshold = 0.1f;

	Animator _Animator;
	bool _ControllingPlayer;

	public bool ControllingPlayer
	{
		get { return _ControllingPlayer; }
		set
		{
			if (_ControllingPlayer != value)
			{
				GetComponent<Hero>().SetCoroutine(GetComponent<Hero>().MonitorForAttacksOnTargets());
				GetComponent<CharController>().ConstrainAnimationDeltaToNavmesh = value;
				_ControllingPlayer = value;
			}
		}
	}

	void Awake()
	{
		_Animator = GetComponent<Animator>();
	}

	// Fixed update is called in sync with physics
	private void FixedUpdate()
	{
		if (GetComponent<Hero>().Stats.HP > 0.0f)
		{
			// Read inputs
			float h = CrossPlatformInputManager.GetAxis("Horizontal");
			float v = CrossPlatformInputManager.GetAxis("Vertical");

			// Are the inputs significant?
			bool shouldStartControllingPlayer = Mathf.Abs(h) > _StartMovingThreshold || Mathf.Abs(v) > _StartMovingThreshold;
			if (shouldStartControllingPlayer)
			{
				ControllingPlayer = true;
			}

			if (ControllingPlayer)
			{
				Vector3 moveSpeed = Vector3.zero;
				// calculate move direction to pass to character
				var cam = CameraManager.Instance.Camera;
				if (cam != null)
				{
					// calculate camera relative direction to move:
					Vector3 camForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
					moveSpeed = v * camForward + h * cam.transform.right;
				}

				// pass all parameters to the character control script
				Move(moveSpeed);
			}
		}
	}

	public void Move(Vector3 move)
	{
		// convert the world relative moveInput vector into a local-relative
		// turn amount and forward amount required to head in the desired
		// direction.
		if (move.magnitude > 1f) move.Normalize();
		move = transform.InverseTransformDirection(move);
		move = Vector3.ProjectOnPlane(move, Vector3.up);
		float turnAmount = Mathf.Atan2(move.x, move.z);
		float forwardAmount = move.z * _MoveSpeed;

		// help the character turn faster (this is in addition to root rotation in the animation)
		transform.Rotate(0, turnAmount * _TurnSpeed * Time.deltaTime, 0);

		// send input and other state parameters to the animator
		_Animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
	}

}
