// ----------------------------------------------------------------------------
//  <copyright file="ActionScope.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.ExecutionPolicy;

/// <summary>
/// Represents predefined scopes for action groups.
/// </summary>
public enum ActionScope
{
    /// <summary>
    /// Indicates a normal code block that do high-level actions.
    /// </summary>
    Application,

    /// <summary>
    /// Indicates a high or low-level action that affects the global variables.
    /// </summary>
    Global,

    /// <summary>
    /// Indicates a low-level (mostly P/Invoke) code block that affects the process attributes.
    /// </summary>
    Process
}
