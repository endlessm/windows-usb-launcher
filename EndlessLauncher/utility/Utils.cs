// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using EndlessLauncher.logger;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static EndlessLauncher.NativeMethods;

namespace EndlessLauncher.utility
{
    public static class Utils
    {
        public static string ByteArrayToHexString(byte[] array, int size)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                sb.Append(array[i].ToString("X2"));
            }

            return sb.ToString();
        }

        public static int CopyStructureToArray<T>(T structure, int offset, byte[] target)
        {
            int size = Marshal.SizeOf(structure);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, target, offset, size);
            Marshal.FreeHGlobal(ptr);

            return size;
        }

        public static Process OpenUrl(string url, string args)
        {
            try
            {
                return Process.Start(url, args);
            }
            catch(Exception ex)
            {
                LogHelper.Log("OpenUrl: {0} Failed: {1}", url, ex.Message);
                return null;
            }
        }

        public static bool ActivateWindow(string cls, string win)
        {
            var window = FindWindow(cls, win);
            if (window != IntPtr.Zero)
            {
                SetForegroundWindow(window);
                if (IsIconic(window))
                {
                    OpenIcon(window);
                }
                return true;
            }
            return false;
        }
    }
}
