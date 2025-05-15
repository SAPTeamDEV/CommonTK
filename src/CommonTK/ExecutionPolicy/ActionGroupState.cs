// ----------------------------------------------------------------------------
//  <copyright file="ActionGroupState.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.ExecutionPolicy;

/// <summary>
/// Represents the current status of a specific action group.
/// </summary>
public enum ActionGroupState
{
    /// <summary>
    /// Indicates a state that an action group has not locked.
    /// </summary>
    Free,

    /// <summary>
    /// Indicates a state that an action group lock temporarily suppressed
    /// </summary>
    Suppressed,

    /// <summary>
    /// Indicates a state that an action group locked by one or more contexts.
    /// </summary>
    Locked
}
