using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
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
}
