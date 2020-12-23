using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropController
	: MonoBehaviour
{
	[SerializeField]
	Transform _RightHandAttachNode;

	Prop _RightHandProp;

	public void AttachProp(Prop prop)
	{
		// Remember that we're attaching a prop
		_RightHandProp = prop;

		// Attach and reset prop xform
		prop.transform.SetParent(_RightHandAttachNode);
		prop.transform.localPosition = Vector3.zero;
		prop.transform.localRotation = Quaternion.identity;
		prop.transform.localScale = Vector3.one;

		// Tell the prop that it was just attached
		prop.PostAttach();
	}

	public void DetachProp()
	{
		// Tell the prop to kill stuff
		if (_RightHandProp != null)
		{
			_RightHandProp.PreDetach();
			_RightHandProp.transform.SetParent(null);
			_RightHandProp = null;
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
