using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTCPServer.Core
{
	/// <summary>
	/// Specifies the type of Task from the thread pool
	/// </summary>
	[Serializable]
	public enum TaskType
	{
		/// <summary>
		/// A task for a user
		/// </summary>
		User = 0b10,
		/// <summary>
		/// A task for receiving bytes from a user
		/// </summary>
		Receiving = 0b1
	}
}
