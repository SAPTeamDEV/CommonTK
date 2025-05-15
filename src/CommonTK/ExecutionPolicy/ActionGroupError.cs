// ----------------------------------------------------------------------------
//  <copyright file="ActionGroupError.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.ExecutionPolicy;

/// <summary>
/// Defines error codes for the action group feature.
/// </summary>
public enum ActionGroupError
{
    /// <summary>
    /// The desired action group was locked by one or more contexts.
    /// </summary>
    Locked = 1,

    /// <summary>
    /// The action group is already locked by this context.
    /// </summary>
    AlreadyLocked,

    /// <summary>
    /// The action group suppressed by a context.
    /// </summary>
    Suppressed,

    /// <summary>
    /// The action group operations for a specific group is not permitted.
    /// </summary>
    AccessDenied,

    /// <summary>
    /// The action group feature only available in global contexts.
    /// </summary>
    NotGlobal,

    /// <summary>
    /// A disposing context can't interact with action groups.
    /// </summary>
    Disposing,

    /// <summary>
    /// The lock of the action group is already suppressed.
    /// </summary>
    AlreadySuppressed,

    /// <summary>
    /// The lock of the action group does not suppressed.
    /// </summary>
    NotSuppressed,

    /// <summary>
    /// Only the suppressor context can relock the action group.
    /// </summary>
    SuppressorRequired
}
