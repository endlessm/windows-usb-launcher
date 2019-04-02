using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessLauncher.model
{
    class SystemVerificationException : EndlessExceptionBase<SystemVerificationErrorCode>
    {
        public SystemVerificationException(SystemVerificationErrorCode errorCode, string message) : base(errorCode, message)
        {
        }
    }
}
