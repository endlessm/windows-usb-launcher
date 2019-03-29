using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessLauncher.model
{
    public class FirmwareSetupException : Exception
    {
        public enum ErrorCode : int
        {
            GenericError = -1,
            OpenProcessTokenError = 1,
            LookupPrivilegeError,
            AdjustTokenPrivilegeError,
            EspVolumeNotFoundError,
            GetBootBorderError,
            NoExistingUefiEntriesError,
            GetPartitionEspInfoError,
            FindFreeUefiEntryError,
            CreateNewUefiEntryError,
            AddToBootOrderError,
            SetBootNextError,
            BiosModeLegacy,
        };

        public FirmwareSetupException(ErrorCode errorCode, string message) : base(message)
        {
            this.Code = errorCode;
        }

        public ErrorCode Code
        {
            get;
            private set;
        }

    }
}
