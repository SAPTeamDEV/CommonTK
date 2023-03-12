using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Console
{
    public enum ConsoleLaunchMode
    {
        /// <summary>
        /// Allocates a Console for Executable using AllocConsole Win32 api P/Invoke call.
        /// </summary>
        Allocation,

        /// <summary>
        /// Attaches to Application Caller Process.
        /// </summary>
        AttachParent,

        /// <summary>
        /// Creates a Dedicated cmd Process, then kill it and uses it's Process Window.
        /// </summary>
        AttachProcess
    }
}
