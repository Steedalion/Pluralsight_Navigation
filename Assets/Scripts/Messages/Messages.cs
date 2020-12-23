using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Messages
{
	/// <summary>
	/// Simple message base interface.
	/// All we care about is a fast way to identify the message type and some name for debugging purposes!
	/// </summary>
	public interface IMessage 
		: System.IDisposable
	{
		string Name { get; }
		int Id { get; }
	}

	/// <summary>
	/// This little static class will give us unique message type ids for quick lookup!
	/// </summary>
	public static class MessageIdGenerator
	{
		static int _NextId = 1;
		public static int NextId
		{
			get { return _NextId++; }
		}
	}

	/// <summary>
	/// Helper class to create a pool of messages of a given type, so we don't constantly allocate memory
	/// </summary>
	public class PooledMessage<T>
		: IMessage
		where T : class, new()
	{
		static Queue<T> _Pool;
		const int _PoolInitialSize = 10;

		static string _Name;
		static int _Id;

		/// <summary>
		/// Fetch the user-friendly name of this message
		/// </summary>
		string IMessage.Name
		{
			get { return _Name; }
		}

		/// <summary>
		/// Fetch the unique id of this message type!
		/// </summary>
		public static int Id
		{
			get { return _Id; }
		}

		/// <summary>
		/// Fetch the unique type id of this message
		/// </summary>
		int IMessage.Id
		{
			get { return _Id; }
		}

		static PooledMessage()
		{
			_Id = MessageIdGenerator.NextId;
			_Name = typeof(T).Name;
			_Pool = new Queue<T>();

			// Prime the pools!
			for (int i = 0; i < _PoolInitialSize; ++i)
			{
				_Pool.Enqueue(new T());
			}
		}

		/// <summary>
		/// Grab a message for use
		/// </summary>
		protected static T Create()
		{
			T ret = null;
			if (_Pool.Count == 0)
			{
				ret = new T();
			}
			else
			{
				ret = _Pool.Dequeue();
			}
			return ret;
		}

		/// <summary>
		/// Return the message to the pool
		/// </summary>
		void System.IDisposable.Dispose()
		{
			Recycle(this);
		}

		static void Recycle(PooledMessage<T> action)
		{
			// Return to the pool!
			_Pool.Enqueue(action as T);
		}
	}
}
