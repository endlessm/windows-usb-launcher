// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using System;

namespace EndlessLauncher.model
{
    public enum FirmwareSetupErrorCode : int
    {
        NoError = int.MinValue,
        GenericFirmwareError = -1,
        OpenProcessTokenError = 1,
        LookupPrivilegeError,
        AdjustTokenPrivilegeError,
        EspPartitionNotFoundError,
        GetBootBorderError,
        NoExistingUefiEntriesError,
        GetPartitionEspInfoError,
        FindFreeUefiEntryError,
        CreateNewUefiEntryError,
        AddToBootOrderError,
        SetBootNextError,
        FirmwareServiceInitializationError
    };

    public enum SystemVerificationErrorCode : int
    {
        NoError = int.MinValue,
        GenericVerificationError = 100,
        Not64BitSystem,
        UnsupportedFirmware,
        NoAdminRights,
        UnsupportedOS,
        InsufficientRAM,
        SingleCoreProcessor,
        UsbDeviceNotFound,
        UsbPortNotFound,
        NotUSB30Port,
        NoUSBPortsFound,
        UnsupportedResolution
    };

    public class EndlessErrorEventArgs<T> : EventArgs
    {
        public T ErrorCode
        {
            get;
            set;
        }
    }
}
