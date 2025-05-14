using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
    internal class EmptyStatusProvider : IStatusProvider
    {
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
