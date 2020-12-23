using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AI;

/// <summary>
/// Bake the navmesh for the scene, but first combine collision meshes
/// This should help walkability
/// </summary>
public static class BakeNavmesh
{
	public static void Bake()
	{
		//// Create temporary object for all Collision Objects in the scene
		//List<GameObject> tempColObjects = new List<GameObject>();
		//List<GameObject> collisionObjects = new List<GameObject>();
		//foreach (var obj in GameObject.FindObjectsOfType<CollisionObjects>())
		//{
		//	// Remember the gameObject
		//	collisionObjects.Add(obj.gameObject);

		//	// For each one, we'll create 2 objects, one for walkable, one for non-walkable
		//	var walkable = new GameObject("temp_Walkable");
		//	var walkableMeshFilter = walkable.AddComponent<MeshFilter>();
		//	var walkableMeshRenderer = walkable.AddComponent<MeshRenderer>();
		//	//walkable.hideFlags = HideFlags.HideAndDontSave;
		//	tempColObjects.Add(walkable);

		//	// Create the mesh!
		//	Mesh walkableMesh = new Mesh();
		//	List<CombineInstance> instances = new List<CombineInstance>();
		//	foreach (var filter in obj.GetComponentsInChildren<MeshFilter>())
		//	{
		//		var instance = new CombineInstance();
		//		instance.mesh = filter.sharedMesh;
		//		instance.transform = filter.transform.localToWorldMatrix;
		//		instances.Add(instance);
		//	}
		//	walkableMesh.CombineMeshes(instances.ToArray());
		//	walkableMesh.name = "Combined Mesh";

		//	// Assign to the new object
		//	walkableMeshFilter.sharedMesh = walkableMesh;

		//	// Disable the previous Object!
		//	obj.gameObject.SetActive(false);
		//}

		//NavMeshBuilder.BuildNavMesh();

		//// Clean up
		//foreach (var obj in tempColObjects)
		//{
		//	var mesh = obj.GetComponent<MeshFilter>().sharedMesh;
		//	obj.GetComponent<MeshFilter>().sharedMesh = null;
		//	GameObject.DestroyImmediate(mesh);
		//	GameObject.DestroyImmediate(obj);
		//}
	}
}
