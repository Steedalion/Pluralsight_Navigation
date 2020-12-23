using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtils
{
	public static bool ComputeLocationForDistanceOnPath(Vector3[] corners, float parameter, float distance, out float outParam, out Vector3 outPosition)
	{
		// Find the segment
		float remainingDistance = distance;
		int currentSegmentIndex = Mathf.FloorToInt(parameter);
		float currentSegmentPercent = parameter - currentSegmentIndex;
		while (remainingDistance >= 0.0f && currentSegmentIndex < corners.Length - 1)
		{
			Vector3 start = corners[currentSegmentIndex];
			Vector3 end = corners[currentSegmentIndex + 1];
			float segmentDistance = Vector3.Distance(start, end);
			if (segmentDistance > remainingDistance)
			{
				// Compute the point and we're done!
				float percent = remainingDistance / segmentDistance;
				float newPercent = percent + currentSegmentPercent;
				if (newPercent < 1.0f)
				{
					outParam = (float)currentSegmentIndex + newPercent;
					outPosition = Vector3.Lerp(start, end, newPercent);
					return true;
				}
				else
				{
					// Segment not long enough!
					remainingDistance -= segmentDistance * (1.0f - currentSegmentPercent);
					currentSegmentIndex++;
					currentSegmentPercent = 0.0f;
				}
			}
			else
			{
				// Move on to next segment
				remainingDistance -= segmentDistance * (1.0f - currentSegmentPercent);
				currentSegmentIndex++;
				currentSegmentPercent = 0.0f;
			}
		}

		// We're run past the end and haven't covered all the distance!
		outParam = corners.Length - 1;
		outPosition = corners[corners.Length - 1];
		return false;
	}

	public static float ComputePathLength(Vector3[] corners)
	{
		// Find the segment that the current parameter falls in
		float sumDistance = 0.0f;
		int currentSegmentIndex = 0;

		// Add up the distances in all the segments, accounting for the starting parameter
		for (int i = currentSegmentIndex; i < corners.Length - 1; ++i)
		{
			// Grab the start and end of the current segment
			Vector3 start = corners[i];
			Vector3 end = corners[i + 1];

			// compute total segment distance!
			float segmentDistance = Vector3.Distance(start, end);

			// Add up
			sumDistance += segmentDistance;
		}
		return sumDistance;
	}

	public static float ComputeDistanceToTarget(Vector3[] corners, float parameter)
	{
		// Find the segment that the current parameter falls in
		float sumDistance = 0.0f;
		int currentSegmentIndex = Mathf.FloorToInt(parameter);

		// Add up the distances in all the segments, accounting for the starting parameter
		float currentStartPercent = parameter - currentSegmentIndex;
		for (int i = currentSegmentIndex; i < corners.Length - 1; ++i)
		{
			// Grab the start and end of the current segment
			Vector3 start = corners[i];
			Vector3 end = corners[i + 1];

			// compute total segment distance!
			float segmentDistance = Vector3.Distance(start, end);

			// Add up
			sumDistance += segmentDistance * (1.0f - currentStartPercent);
			currentStartPercent = 0.0f;
		}
		return sumDistance;
	}

	public static bool ComputeLocationForDistanceFromGoal(Vector3[] corners, float distance, out Vector3 position, out float parameter)
	{
		if (distance == 0.0f)
		{
			position = corners[corners.Length - 1];
			parameter = corners.Length - 1;
			return true;
		}
		else
		{
			// Find the segment that the current parameter falls in
			float sumDistance = 0.0f;
			for (int i = corners.Length - 2; i >= 0; --i)
			{
				Vector3 start = corners[i];
				Vector3 end = corners[i + 1];

				// compute segment distance
				float segmentDistance = Vector3.Distance(start, end);

				if (sumDistance + segmentDistance >= distance)
				{
					float percent = (distance - sumDistance) / segmentDistance;
					position = Vector3.Lerp(end, start, percent);
					parameter = i + 1.0f - percent;
					return true;
				}

				sumDistance += segmentDistance;
			}

			position = corners[0];
			parameter = 0.0f;
			return false;
		}
	}

}
