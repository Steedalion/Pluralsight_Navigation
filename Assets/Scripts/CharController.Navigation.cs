using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Coroutines;

public partial class CharController
	: MonoBehaviour
{
	public IEnumerable<Instruction> RunToCharacterCr(Character target, float distanceFromCharacter, float speed)
	{
		float smallDistance = 0.1f;
		System.Func<bool> farEnough = () =>
		{
			float dist = Vector3.Distance(target.transform.position, transform.position);
			return dist > (distanceFromCharacter + smallDistance);
		};

		if (farEnough.Invoke())
		{
			float thresholdDistance = 0.1f;

			using (var runToCharWhileFarEnough = Flow.RepeatWhile(RunToCharacterUnlessCharacterMoved(target, distanceFromCharacter, speed, thresholdDistance), farEnough).GetEnumerator())
			{
				while (runToCharWhileFarEnough.MoveNext())
					yield return runToCharWhileFarEnough.Current;
			}
		}
	}

	public IEnumerable<Instruction> RunToCharacterUnlessCharacterMoved(Character target, float distanceFromCharacter, float speed, float thresholdDistance)
	{
		// Remember the starting position of the target character
		Vector3 targetStartPos = target.transform.position;

		// This will tell us if the target moved too far!
		System.Func<bool> noRepathNeeded = () =>
		{
			float distance = Vector3.Distance(target.transform.position, targetStartPos);
			return distance < thresholdDistance;
		};

		// Run to the target unless they move too much!
		using (var run = Coroutines.Flow.ExecuteWhile(RunToCr(targetStartPos, distanceFromCharacter, speed), noRepathNeeded).GetEnumerator())
		{
			while (run.MoveNext())
				yield return run.Current;
		}
	}

	public IEnumerable<Instruction> RunToCr(Vector3 target, float distanceFromGoal, float speed)
	{
		// Build the path!
		NavMeshPath path = new NavMeshPath();
		NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);

		// And start following it!
		if (path.status != NavMeshPathStatus.PathInvalid)
		{
			var pathCorners = path.corners;

			if (NavMeshUtils.ComputePathLength(pathCorners) > 0.1f)
			{
				float currentPathParam = 0.0f;

				// Figure out point at which to start slowing down
				Vector3 slowDownStart = pathCorners[pathCorners.Length - 1];
				float slowDownParameter;
				NavMeshUtils.ComputeLocationForDistanceFromGoal(pathCorners, distanceFromGoal + _StoppingDistance, out slowDownStart, out slowDownParameter);

				// And the point at which to stop
				Vector3 slowDownEnd;
				float ignoreParam = 0.0f;
				if (NavMeshUtils.ComputeLocationForDistanceFromGoal(pathCorners, distanceFromGoal, out slowDownEnd, out ignoreParam))
				{
					// Rotate towards the path
					Vector3 pos = Vector3.zero;
					if (NavMeshUtils.ComputeLocationForDistanceOnPath(pathCorners, 0.0f, 0.1f, out ignoreParam, out pos))
					{
						// Rotate towards the path
						Vector3 currentDirection = pos - transform.position;
						currentDirection.y = 0.0f;

						// Maybe we're close enough we don't need to snap to the angle!
						float angle = Mathf.Abs(Vector3.Angle(currentDirection, transform.forward));
						if (angle > _SnapAngleThreshold)
						{
							transform.rotation = Quaternion.LookRotation(currentDirection);
						}

						// Tell the animation to start running
						_Animator.SetFloat(_ForwardId, speed);
						_ApplyRootMotion = false; // We'll modify and apply it ourselves!
						//_Animator.SetLayerWeight(1, 0.0f);

						// Follow the path!
						try
						{
							// Wait a frame before starting to move
							yield return Coroutines.Flow.WaitForAnimatorUpdate;

							while (currentPathParam < slowDownParameter)
							{
								// Compute next position along path
								Vector3 localDelta = transform.InverseTransformVector(_Animator.deltaPosition);
								if (localDelta.z > 0.0f)
								{
									float newParam = 0.0f;
									Vector3 newPos = Vector3.zero;
									if (NavMeshUtils.ComputeLocationForDistanceOnPath(pathCorners, currentPathParam, localDelta.z, out newParam, out newPos))
									{
										// Snap to heightmesh height
										NavMeshHit hit;
										NavMesh.SamplePosition(newPos, out hit, float.MaxValue, NavMesh.AllAreas);
										newPos.y = hit.position.y;

										// Orient actor towards path!
										Vector3 newDirection = newPos - transform.position;
										newDirection.y = 0.0f;
										if (newDirection.sqrMagnitude > 0.0001f)
										{
											// Only update direction if we were able to compute one!
											currentDirection = newDirection.normalized;
										}
										Vector3 newForward = Vector3.RotateTowards(transform.forward, currentDirection, _RotationSpeed * Mathf.Deg2Rad * Time.deltaTime, float.MaxValue);

										// Move the actor to that new position
										transform.position = newPos;
										transform.rotation = Quaternion.LookRotation(newForward);

										// Update the parameter to match!
										currentPathParam = newParam;
									}
									else
									{
										Debug.LogWarning("Can't update position on path, " + currentPathParam + ", delta: " + localDelta.z);
									}
								}
								// Else the animation is moving backwards, ignore it!

								// Wait until enxt frame!
								yield return Coroutines.Flow.WaitForAnimatorUpdate;
							}

							// Tell animator to stop running
							_Animator.SetFloat(_ForwardId, 0.0f);

							// Tween towars the stopping point
							slowDownStart = transform.position;

							float slowDownTimer = 0.0f;
							while (slowDownTimer < _StoppingTransitionDuration)
							{
								// Update the timer!
								slowDownTimer += Time.deltaTime;

								float interpolationPercent = slowDownTimer / _StoppingTransitionDuration;
								if (interpolationPercent >= 1.0f)
								{
									transform.position = slowDownEnd;

									// We would break anyway, but this skips one frame!
									break;
								}
								else
								{
									Vector3 newPos = Vector3.Lerp(slowDownStart, slowDownEnd, interpolationPercent);

									// Snap to navmesh height
									NavMeshHit hit;
									NavMesh.SamplePosition(newPos, out hit, float.MaxValue, NavMesh.AllAreas);
									newPos.y = hit.position.y;

									// Orient actor towards path!
									Vector3 newDirection = newPos - transform.position;
									newDirection.y = 0.0f;
									if (newDirection.sqrMagnitude > 0.0001f)
									{
										// Only update direction if we were able to compute one!
										currentDirection = newDirection.normalized;
									}
									Vector3 newForward = Vector3.RotateTowards(transform.forward, currentDirection, _RotationSpeed * Mathf.Deg2Rad * Time.deltaTime, float.MaxValue);

									// Move the actor to that new position
									transform.position = newPos;
									transform.rotation = Quaternion.LookRotation(newForward);
								}

								yield return Coroutines.Flow.WaitForAnimatorUpdate;
							}
						}
						finally
						{
							// Tell animator to stop running
							_Animator.SetFloat(_ForwardId, 0.0f);
							_ApplyRootMotion = true; // Make sure to reset it!
							//_Animator.SetLayerWeight(1, 1.0f);
						}
					}
					else
					{
						// Error, can't even get a direction vector from the path
						Debug.LogWarning("Can't orient towards path");
					}
				}
				else
				{
					Debug.LogWarning("Can't compute stopping point");
				}
			}
			// Else the path is really short!
		}
		else
		{
			Debug.LogWarning("Can't compute path");
		}
	}
}
