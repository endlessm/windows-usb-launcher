using EndlessLauncher.logger;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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

        public static void OpenUrl(string url, string args)
        {
            try
            {
                Process.Start(url, args);
            }
            catch(Exception ex)
            {
                LogHelper.Log("OpenUrl: {0} Failed: {1}", url, ex.Message);
            }
        }
    }
}
