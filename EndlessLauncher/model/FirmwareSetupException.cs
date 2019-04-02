using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessLauncher.model
{
    public class FirmwareSetupException : EndlessExceptionBase<FirmwareSetupErrorCode>
    {
        public FirmwareSetupException(FirmwareSetupErrorCode errorCode, string message) : base(errorCode, message)
        {
        }
    }
}
