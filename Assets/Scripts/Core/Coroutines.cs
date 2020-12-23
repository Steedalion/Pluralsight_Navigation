using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coroutines are functions that execute over time (multiple frames)
/// We use this little framework to process them. In this file, we have return instructions
/// that tell the coroutine 'executor' (typically a monobehiour) what to do.
/// </summary>
namespace Coroutines
{
	/// <summary>
	/// Base interface for returned instructions that tell the coroutine executor what to do!
	/// </summary>
	public interface Instruction
	{
		// Nothing for now!
	}

	/// <summary>
	/// This tells the executor to wait until the next monobehaviour update, yield returning null will also do that!
	/// </summary>
	public class WaitForUpdate
		: Instruction
	{

	}

	/// <summary>
	/// This tells the executor to wait until the next lateupdate
	/// </summary>
	public class WaitForLateUpdate
		: Instruction
	{

	}

	/// <summary>
	/// This tells the executor to wait until the next onanimatorupdate
	/// </summary>
	public class WaitForAnimatorUpdate
		: Instruction
	{

	}

	public class CoroutineSite
	{
		IEnumerator<Instruction> _CurrentCoroutine;

		public void Update()
		{
			// Only update the coroutine if we're told to!
			if (_CurrentCoroutine != null && (_CurrentCoroutine.Current == null || _CurrentCoroutine.Current is WaitForUpdate))
			{
				UpdateCurrentCoroutine();
			}
		}

		public void LateUpdate()
		{
			// Only update the coroutine if we're told to!
			if (_CurrentCoroutine != null && _CurrentCoroutine.Current is WaitForLateUpdate)
			{
				UpdateCurrentCoroutine();
			}
		}

		public void OnAnimatorMove()
		{
			// Only update the coroutine if we're told to!
			if (_CurrentCoroutine != null && _CurrentCoroutine.Current is WaitForAnimatorUpdate)
			{
				UpdateCurrentCoroutine();
			}
		}

		public void SetCoroutine(IEnumerable<Instruction> coroutine)
		{
			// Kill any previous command!
			CancelCoroutine();

			// Initialize the command coroutine!
			_CurrentCoroutine = coroutine.GetEnumerator();
		}

		public void CancelCoroutine()
		{
			if (_CurrentCoroutine != null)
			{
				_CurrentCoroutine.Dispose();
				_CurrentCoroutine = null;
			}
		}

		void UpdateCurrentCoroutine()
		{
			if (_CurrentCoroutine != null)
			{
				_CurrentCoroutine.MoveNext();
			}
		}
	}

	/// <summary>
	/// Utility methods for controlling execution flow
	/// Things such as calling a sub-coroutine, or executing multiple sub-coroutines in parallel
	/// </summary>
	public class Flow
	{
		public static WaitForUpdate WaitForUpdate
		{
			get;
			private set;
		}

		public static WaitForLateUpdate WaitForLateUpdate
		{
			get;
			private set;
		}

		public static WaitForAnimatorUpdate WaitForAnimatorUpdate
		{
			get;
			private set;
		}

		static Flow()
		{
			WaitForUpdate = new WaitForUpdate();
			WaitForLateUpdate = new WaitForLateUpdate();
			WaitForAnimatorUpdate = new WaitForAnimatorUpdate();
		}

		/// <summary>
		/// Utility method to execute a sub-coroutine while a predicate is true
		/// </summary>
		public static IEnumerable<Instruction> WaitForSeconds(float duration, Instruction yieldInstruction = null)
		{
			float timer = 0.0f;
			while (timer < duration)
			{
				yield return yieldInstruction;
				timer += Time.deltaTime;
			}
		}

		/// <summary>
		/// Utility method to execute a sub-coroutine while a predicate is true
		/// </summary>
		public static IEnumerable<Instruction> ExecuteWhile(IEnumerable<Instruction> coroutine, System.Func<bool> earlyOutPredicate)
		{
			using (var instance = coroutine.GetEnumerator())
			{
				while (earlyOutPredicate.Invoke() && instance.MoveNext())
					yield return instance.Current;
			}
		}

		public static IEnumerable<Instruction> ExecuteOrWaitUntil(IEnumerable<Instruction> code, Instruction skipInstruction = null)
		{
			using (var instance = code.GetEnumerator())
			{
				// See if it yielded at least once!
				if (instance.MoveNext())
				{
					// Yes, now we can yield until the end
					yield return instance.Current;
					while (instance.MoveNext())
					{
						yield return instance.Current;
					}
				}
				else
				{
					// No, skip a frame
					yield return skipInstruction;
				}
			}
		}

		/// <summary>
		/// Utility method to execute a repeat a coroutine while a predicate is true!
		/// </summary>
		public static IEnumerable<Instruction> RepeatWhile(IEnumerable<Instruction> coroutine, System.Func<bool> predicate)
		{
			while (predicate.Invoke())
			{
				using (var instance = coroutine.GetEnumerator())
				{
					while (instance.MoveNext())
						yield return instance.Current;
				}
			}
		}

		/// <summary>
		/// Utility method to execute two subroutines together
		/// </summary>
		public static IEnumerable<Instruction> MasterSlave(IEnumerable<Instruction> master, IEnumerable<Instruction> slave)
		{
			using (var instanceMaster = master.GetEnumerator())
			{
				bool slaveCompleted = false;
				using (var instanceSlave = slave.GetEnumerator())
				{
					while (instanceMaster.MoveNext())
					{
						if (!slaveCompleted)
						{
							slaveCompleted = !instanceSlave.MoveNext();
						}

						// We should really be looking at the return value from slave as well !!!
						yield return instanceMaster.Current;
					}
				}
			}
		}
	}
}

