using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// The exception that is thrown when there is an attempt to access a locked action group.
    /// </summary>
    public class ActionGroupException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
        /// </summary>
        public ActionGroupException()
            : base("The action group is not accessible.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ActionGroupException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
        public ActionGroupException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ActionGroupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
