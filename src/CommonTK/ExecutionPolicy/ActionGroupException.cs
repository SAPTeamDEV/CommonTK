// ----------------------------------------------------------------------------
//  <copyright file="ActionGroupException.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.ExecutionPolicy;

/// <summary>
/// The exception that is thrown when there is an attempt to access a locked action group.
/// </summary>
public class ActionGroupException : Exception
{
    /// <summary>
    /// Gets the generic error messages.
    /// </summary>
    public static Dictionary<int, string> Messages => new()
    {
        [(int)ActionGroupError.Locked] = "The action group is locked.",
        [(int)ActionGroupError.AlreadyLocked] = "The action group is already locked by this context.",
        [(int)ActionGroupError.Suppressed] = "The action group is suppressed",
        [(int)ActionGroupError.AccessDenied] = "The action group operations is not permitted.",
        [(int)ActionGroupError.NotGlobal] = "The action group feature only available in global contexts.",
        [(int)ActionGroupError.Disposing] = "A disposing context can't interact with action groups.",
        [(int)ActionGroupError.AlreadySuppressed] = "The lock of the action group is already suppressed.",
        [(int)ActionGroupError.NotSuppressed] = "The lock of the action group does not suppressed.",
        [(int)ActionGroupError.SuppressorRequired] = "Only the suppressor context can relock the action group."
    };

    /// <summary>
    /// Gets the error code that describes the problem.
    /// </summary>
    public int ErrorCode { get; }

    /*
    /// <summary>
    /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
    /// </summary>
    public ActionGroupException()
        : base("There is a problem with action group.")
    {
    }
    */

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
    /// </summary>
    /// <param name="errorCode">
    /// The error code that describes the problem.
    /// </param>
    public ActionGroupException(int errorCode)
        : base(Messages[errorCode])
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
    /// </summary>
    /// <param name="errorCode">
    /// The error code that describes the problem.
    /// </param>
    public ActionGroupException(ActionGroupError errorCode)
        : this((int)errorCode)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
    /// </summary>
    /// <param name="message">
    /// The error message.
    /// </param>
    /// <param name="errorCode">
    /// The error code that describes the problem.
    /// </param>
    public ActionGroupException(string message, int errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
    /// </summary>
    /// <param name="message">
    /// The error message.
    /// </param>
    /// <param name="errorCode">
    /// The error code that describes the problem.
    /// </param>
    public ActionGroupException(string message, ActionGroupError errorCode)
        : base(message)
    {
        ErrorCode = (int)errorCode;
    }

    /*
    /// <summary>
    /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ActionGroupException(string message)
        : base(message)
    {
    }
    */

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionGroupException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ActionGroupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
