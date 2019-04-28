namespace SimpleTCPServer.Core
{
    /// <summary>
    /// A structure for storing the settings of the TCP server
    /// </summary>
	[System.Serializable]
    public class TCPServerConfig
    {
        /// <summary>
        /// The byte array size
        /// </summary>
        public int BytesSize { get; set; }
		/// <summary>
		/// Keep a collection of the connected clients
		/// </summary>
		public bool KeepListOfClients { get; set; }
    }
}