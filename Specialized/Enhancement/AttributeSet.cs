using System;
using System.Collections.Generic;

namespace Woodpecker.Specialized.Enhancement
{
    public abstract class AttributeSet
    {
        #region Fields
        /// <summary>
        /// A dictionary with string objects as keys and 'object' objects as values.
        /// </summary>
        protected Dictionary<string, object> Attributes = new Dictionary<string, object>();
        #endregion

        #region Methods
        /// <summary>
        /// Sets a attribute with a value. If the attribute is already set, the old attribute will be removed.
        /// </summary>
        /// <param name="Attribute">The attribute to set.</param>
        /// <param name="Value">The value of the attribute to set.</param>
        public void setAttribute(string Attribute, object Value)
        {
            if (this.Attributes.ContainsKey(Attribute))
                this.Attributes.Remove(Attribute);

            if(Value != null && Value != DBNull.Value) // Valid value all the way
                this.Attributes.Add(Attribute, Value);
        }
        /// <summary>
        /// Tries to unset a given attribute.
        /// </summary>
        /// <param name="Attribute">The attribute to unset.</param>
        public void unsetAttribute(string Attribute)
        {
            this.Attributes.Remove(Attribute);
        }
        /// <summary>
        /// Returns true if the attribute collection has a value set for a given attribute.
        /// </summary>
        /// <param name="Attribute">The attribute to check.</param>
        public bool hasSetAttribute(string Attribute)
        {
            return this.Attributes.ContainsKey(Attribute);
        }
        /// <summary>
        /// Tries to return the value of a given attribute as an object. Null is returned if the attribute is not set.
        /// </summary>
        /// <param name="Attribute">The attribute to retrieve the value of.</param>
        public object getAttribute(string Attribute)
        {
            if (this.hasSetAttribute(Attribute))
                return this.Attributes[Attribute];
            else
                return null;
        }
        /// <summary>
        /// Returns the value of a given attribute as a string. If the given attribute isn't set, a blank string is returned.
        /// </summary>
        /// <param name="Attribute">The attribute to retrieve the value of.</param>
        public string getStringAttribute(string Attribute)
        {
            object obj = this.getAttribute(Attribute);
            if (obj != null)
                return (string)obj;
            else
                return "";
        }
        #endregion
    }
}
