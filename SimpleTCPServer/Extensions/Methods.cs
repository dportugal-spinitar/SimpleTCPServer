using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SimpleTCPServer.Extensions
{
    /// <summary>
    /// The class for the methods of converting byte arrays and parsing IP addresses
    /// </summary>
    public static class Methods
    {
        /// <summary>
        /// Converts a string with an ip and port to an IPEndpoint
        /// </summary>
        /// <param name="endPoint">The string to be converted</param>
        /// <returns>Returns the string converted to an IPEndpoint</returns>
        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out int port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }
        /// <summary>
        /// Converts the object to a byte array
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>Returns the object converted to a byte array</returns>
        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        /// <summary>
        /// Converts a byte array to any type
        /// </summary>
        /// <typeparam name="T">The type to convert the byte array to</typeparam>
        /// <param name="arrBytes">The byte array</param>
        /// <returns>Returns the byte array converted to the specified type</returns>
        public static T FromByteArray<T>(this byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);

            return (T)obj;
        }
    }
}
