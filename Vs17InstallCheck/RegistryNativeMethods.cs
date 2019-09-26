// Copyright (c) Adrian Sims 2019
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


namespace Vs17InstallCheck
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    internal static class RegistryNativeMethods
    {
        [Flags]
        public enum RegSAM
        {
            AllAccess = 0x000f003f,
            KEY_CREATE_LINK = 0x0020,
            KEY_CREATE_SUB_KEY = 0x0004,
            KEY_ENUMERATE_SUB_KEYS = 0x0008,
            KEY_EXECUTE = 0x20019,
            KEY_NOTIFY = 0x0010,
            KEY_QUERY_VALUE = 0x0001,
            KEY_READ = 0x20019,
            KEY_SET_VALUE = 0x0002,
            KEY_WOW64_32KEY = 0x0200,
            KEY_WOW64_64KEY = 0x0100,
            KEY_WRITE = 0x20006,
        }

        private const int REG_PROCESS_APPKEY = 0x00000001;

        // approximated from pinvoke.net's RegLoadKey and RegOpenKey
        // NOTE: changed return from long to int so we could do Win32Exception on it
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegLoadAppKey(String hiveFile, out int hKey, RegSAM samDesired, int options, int reserved);

        public static int RegLoadAppKey(String hiveFile)
        {
            int hKey;
            int rc = RegLoadAppKey(hiveFile, out hKey, RegSAM.KEY_ENUMERATE_SUB_KEYS | RegSAM.KEY_QUERY_VALUE | RegSAM.KEY_READ, 0, 0);

            if (rc != 0)
            {
                throw new Win32Exception(rc, "Failed during RegLoadAppKey of file " + hiveFile);
            }

            return hKey;
        }
    }
}
