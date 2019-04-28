namespace SimpleTCPServer.Logging
{
    /// <summary>
    /// A log message
    /// </summary>
    public interface ILogMessage
    {
        /// <summary>
        /// The content
        /// </summary>
        string Content { get; }
        /// <summary>
        /// The source of the log message
        /// </summary>
        string Source { get; }
        /// <summary>
        /// The type of the log message
        /// </summary>
        LogMessageType Type { get; }
    }
    /// <summary>
    /// The log message's type
    /// </summary>
	[System.Serializable]
    public enum LogMessageType : int
    {
        /// <summary>
        /// A user joined
        /// </summary>
        ClientConnected,
        /// <summary>
        /// Bytes are received
        /// </summary>
        BytesReceived,
        /// <summary>
        /// The server is ready/listening for connections
        /// </summary>
        ServerReady,
        /// <summary>
        /// A client has left
        /// </summary>
        ClientDisconnected
    }
}
