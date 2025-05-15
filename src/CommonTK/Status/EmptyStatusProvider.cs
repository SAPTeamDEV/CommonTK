using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK.Status
{
    internal class EmptyStatusProvider : IStatusProvider
    {
        public static EmptyStatusProvider Instance = new EmptyStatusProvider();

        public void Clear()
        {

        }

        public void Dispose()
        {

        }

        public StatusIdentifier Write(string message)
        {
            return StatusIdentifier.Empty;
        }
    }
}
