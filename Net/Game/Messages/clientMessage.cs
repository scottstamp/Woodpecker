using System;
using System.Collections.Generic;

using Woodpecker.Specialized.Encoding;

namespace Woodpecker.Net.Game.Messages
{
    /// <summary>
    /// Represents a client>server message for the Habbo FUSE protocol.
    /// </summary>
    public class clientMessage
    {
        #region Fields
        /// <summary>
        /// The message ID of this message.
        /// </summary>
        public readonly int ID;
        /// <summary>
        /// The content of this message.
        /// </summary>
        public string Content;
        #endregion

        #region Constructors
        public clientMessage(string Data)
        {
            this.ID = base64Encoding.Decode(Data.Substring(0, 2));
            this.Content = Data.Substring(2);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retrieves a certain parameter from the message's content. If the parameter is not found or any errors occur, then "" is returned.
        /// </summary>
        /// <param name="paramID">The ID of the parameter to retrieve.</param>
        public string getStructuredParameter(int paramID)
        {
            return base64Encoding.getStructuredParameter(this.Content, paramID);
        }
        /// <summary>
        /// Searches through the message content for the xxth parameter and returns it. If not found, then "" is returned.
        /// </summary>
        /// <param name="parameterLocation">The parameter to look for, 0 will return the first parameter, 1 the second, 2 the third etc.</param>
        public string getParameter(int parameterLocation)
        {
            return base64Encoding.getParameter(this.Content, parameterLocation);
        }
        /// <summary>
        /// Wire decodes the message content and returns it as an integer.
        /// </summary>
        /// <returns></returns>
        public int getNextWiredParameter()
        {
            try
            {
                return wireEncoding.Decode(ref this.Content);
            }
            catch { return 0; }
        }
        /// <summary>
        /// Decodes the whole message content (assumed that it's a wired string of encoded numbers) to an integer array. An empty array is returned on errors.
        /// </summary>
        public int[] getWiredParameters()
        {
            return wireEncoding.decodeWire(this.Content);
        }
        public string[] getMixedParameters()
        {
            return mixedEncoding.getMixedParameters(this.Content);
        }
        public int[] getNumericMixedParameters()
        {
            return mixedEncoding.getMixedParametersAsIntegers(this.Content);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the ID of this message encoded in Base64.
        /// </summary>
        public string encodedID
        {
            get { return base64Encoding.Encode(this.ID); }
        }
        /// <summary>
        /// The method name of this message.
        /// </summary>
        public string methodName
        {
            get { return clientMessageTargetMethodNames.getMessageTargetMethodName(this.ID); }
        }
        /// <summary>
        /// Returns all parameters in this message as a string array, as indicated by the Base64 headers.
        /// </summary>
        public string[] Parameters
        {
            get { return base64Encoding.getParameters(this.Content); }
        }
        #endregion
    }
}
