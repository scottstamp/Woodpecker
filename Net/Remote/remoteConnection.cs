using System;
using System.Net.Sockets;

using Woodpecker.Core;
using Woodpecker.Specialized.Text;
using Woodpecker.Specialized.Encoding;

namespace Woodpecker.Net.Remote
{
    public class remoteConnection
    {
        #region Fields
        public const int dataByteLength = 512;
        /// <summary>
        /// The Socket object of this remote connection.
        /// </summary>
        private Socket mSocket;
        /// <summary>
        /// The byte array that is used for receiving data.
        /// </summary>
        private byte[] mDataBuffer;
        #endregion

        #region Constructors
        public remoteConnection(Socket Client)
        {
            mSocket = Client;
            mDataBuffer = new byte[dataByteLength];
        }
        #endregion

        #region Methods
        public void waitForData()
        {
            mSocket.BeginReceive(mDataBuffer, 0, dataByteLength, SocketFlags.None, new AsyncCallback(this.dataArrival), null);
        }
        private void dataArrival(IAsyncResult iAr)
        {
            try
            {
                int bytesReceived = mSocket.EndReceive(iAr);
                bool Result = false;

                if (bytesReceived <= dataByteLength)
                {
                    string[] saData = Configuration.charTable.GetString(mDataBuffer, 0, bytesReceived).Split(Convert.ToChar(1));

                    int messageID = -1;
                    Result = (int.TryParse(saData[0], out messageID) && Engine.Net.Remote.handleRequest(messageID, saData));
                }

                byte bRet = (byte)(Result ? 49 : 48); // 1 = OK, 0 = BAD
                mSocket.Send(new byte[] { bRet });

                mSocket.Close();
            }
            catch { }
            finally
            {
                mSocket = null;
                mDataBuffer = null;
            }
        }
        #endregion
    }
}
