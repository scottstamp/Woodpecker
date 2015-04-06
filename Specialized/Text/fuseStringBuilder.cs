using System;
using System.Text;
using System.Globalization;

using Woodpecker.Specialized.Encoding;

namespace Woodpecker.Specialized.Text
{
    /// <summary>
    /// An enhanced System.Text.StringBuilder for creating strings for the FUSE protocol.
    /// </summary>
    public class fuseStringBuilder
    {
        #region Fields
        /// <summary>
        /// The actual content of the string builder.
        /// </summary>
        protected StringBuilder Content;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the System.Text.StringBuilder inside the FUSE stringbuilder.
        /// </summary>
        public fuseStringBuilder()
        {
            this.Content = new StringBuilder();
        }
        #endregion

        #region Methods
        #region Normal appending methods
        /// <summary>
        /// Appends a string to the string builder.
        /// </summary>
        /// <param name="s">The string to append.</param>
        public void Append(string s)
        {
            this.Content.Append(s);
        }
        /// <summary>
        /// Appends an integer as a string to the string builder.
        /// </summary>
        /// <param name="i">The integer to append.</param>
        public void Append(int i)
        {
            this.Content.Append(i);
        }
        /// <summary>
        /// Appends a floating point value to the string builder, after formatting it to 0.00 format.
        /// </summary>
        /// <param name="f">The floating point value to format and append.</param>
        public void Append(float f)
        {
            this.Content.Append(f.ToString("0.00", CultureInfo.InvariantCulture));
        }
        #endregion

        #region Special appending methods
        /// <summary>
        /// Appends a Unicode character to the string builder.
        /// </summary>
        /// <param name="x">The number of the character to append.</param>
        public void appendChar(int x)
        {
            this.Content.Append(Convert.ToChar(x));
        }
        /// <summary>
        /// Appends a wire encoded integer to the string builder.
        /// </summary>
        /// <param name="i">The integer to encode and append.</param>
        public void appendWired(int i)
        {
            this.Content.Append(wireEncoding.Encode(i));
        }
        /// <summary>
        /// Appends a wire encoded boolean to the string builder.
        /// </summary>
        /// <param name="b">The boolean to encode and append.</param>
        public void appendWired(bool b)
        {
            if (b)
                this.Content.Append('I');
            else
                this.Content.Append('H');
        }
        /// <summary>
        /// Appends a value in the 'key'-'value' format, with a given separator. The key-value pair is is closed by char13.
        /// </summary>
        /// <param name="Key">The key of the value to append as a string.</param>
        /// <param name="Value">The value to append.</param>
        /// <param name="Separator">The Unicode character that separates the key and the value.</param>
        public void appendKeyValueParameter(string Key, object Value, char Separator)
        {
            this.Content.Append(Key);
            this.Content.Append(Separator);
            this.Content.Append(Value.ToString());
            this.appendChar(13);
        }
        /// <summary>
        /// Appends a value in the 'key'-'value' format, with ':' as separator. The key-value pair is is closed by char13.
        /// </summary>
        /// <param name="Key">The key of the value to append as a string.</param>
        /// <param name="Value">The value to append.</param>
        /// <seealso>appendKeyValueParameter(string,object,char)</seealso>
        public void appendKeyValueParameter(string Key, object Value)
        {
            this.appendKeyValueParameter(Key, Value, ':');
        }
        /// <summary>
        /// Appends a value followed by a char9 to the string builder.
        /// </summary>
        /// <param name="Value">The value to append.</param>
        public void appendTabbedValue(string Value)
        {
            this.Content.Append(Value);
            this.appendChar(9);
        }
        /// <summary>
        /// Appends a value closed by char2 to the string builder.
        /// </summary>
        /// <param name="Value">The value to append.</param>
        public void appendClosedValue(string Value)
        {
            if(Value != null)
                this.Content.Append(Value);
            this.appendChar(2);
        }
        /// <summary>
        /// Appends a value closed by char13 to the string builder.
        /// </summary>
        /// <param name="Value">The value to append.</param>
        public void appendNewLineValue(string Value)
        {
            if (Value != null)
                this.Content.Append(Value);
            this.appendChar(13);
        }
        /// <summary>
        /// Appends a value followed by a char30 to the string builder.
        /// </summary>
        /// <param name="Value">The value to append.</param>
        public void appendStripValue(string Value)
        {
            this.Content.Append(Value);
            this.appendChar(30);
        }
        /// <summary>
        /// Appends a value followed by a single whitespace ('spacebar') character to the string builder.
        /// <param name="Value">The value to append.</param>
        /// </summary>
        public void appendWhiteSpacedValue(string Value)
        {
            this.Content.Append(Value);
            this.Content.Append(" ");
        }
        #endregion

        #region Other methods
        /// <summary>
        /// Clears the content from the string builder.
        /// </summary>
        public void Clear()
        {
            this.Content = new StringBuilder();
        }
        /// <summary>
        /// Returns the content of the stringbuilder inside this object as a string.
        /// </summary>
        public override string ToString()
        {
            return this.Content.ToString();
        }
        #endregion
        #endregion
    }
}
