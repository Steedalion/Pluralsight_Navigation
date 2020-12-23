using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop
	: MonoBehaviour
{
	[SerializeField]
	Collider _Collider;

	Rigidbody _RigidBody;

	public Collider Collider
	{
		get { return _Collider; }
	}

	void Awake()
	{
		_RigidBody = GetComponent<Rigidbody>();
	}

	// Use this for initialization
	void Start ()
	{
		if (_RigidBody != null)
		{
			_RigidBody.isKinematic = true;
		}
	}

	// Update is called once per frame
	void Update ()
	{
		
	}

	public virtual void PostAttach()
	{
		if (_RigidBody != null)
		{
			_RigidBody.isKinematic = true;
		}
	}

	public virtual void PreDetach()
	{
		if (_RigidBody != null)
		{
			_RigidBody.isKinematic = false;
		}
	}
}
