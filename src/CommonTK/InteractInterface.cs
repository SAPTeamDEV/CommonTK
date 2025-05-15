// ----------------------------------------------------------------------------
//  <copyright file="InteractInterface.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK;

/// <summary>
/// Represent the process interaction interface.
/// </summary>
public enum InteractInterface
{
    /// <summary>
    /// Unsupported of Unknown state of interact interface.
    /// </summary>
    None,

    /// <summary>
    /// Represents the state that process have a Console windows. In this state all outputs must be passed to the console window.
    /// </summary>
    Console,

    /// <summary>
    /// Represents the state that process does not have a console window. In this state all outluts must be processed visually.
    /// </summary>
    UI
}