using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTCPServer.Core
{
	/// <summary>
	/// A holder of a Task, CancellationTokenSource and a TaskType
	/// </summary>
	[Serializable]
	public class TaskHolder : IDisposable
	{
		/// <summary>
		/// The token
		/// </summary>
		
		internal CancellationTokenSource TokenSource { get; }

		/// <summary>
		/// The TaskType
		/// </summary>
		public TaskType Type { get; }

		[NonSerialized]
		private bool isDisposed = false;
		/// <summary>
		/// Disposes this object
		/// </summary>
		public void Dispose()
		{
			if (isDisposed)
				return;
			TokenSource.Dispose();
			isDisposed = true;
		}

		/// <summary>
		/// The constructor of a TaskHolder
		/// </summary>
		/// <param name="source">The source for the cancellation token</param>
		/// <param name="tasktype">The type of task</param>
		public TaskHolder(CancellationTokenSource source, TaskType tasktype)
		{
			TokenSource = source;
			
			Type = tasktype;
		}

	}
}
