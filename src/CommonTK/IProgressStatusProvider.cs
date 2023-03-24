using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK
{
    public interface IProgressStatusProvider : IStatusProvider
    {
        void Write(string message, ProgressBarType type);

        void Increment(int value);
    }
}
