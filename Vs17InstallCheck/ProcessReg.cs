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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace Vs17InstallCheck
{
    internal class ProcessReg
    {
        internal static void Process(List<string> privateRegistryList)
        {
            Console.WriteLine();
            Console.WriteLine(@"Dumping EnterpriseTools\QualityTools\TestTypes from private registries:");
            foreach (var filename in privateRegistryList)
            {
                Console.WriteLine($"    {filename}");
                var directory = Path.GetDirectoryName(filename);
                var lastPart = new DirectoryInfo(directory).Name;

                try
                {
                    if (!File.Exists(filename))
                    {
                        throw new VSPrivateRegException($"File {filename} does not exist.");
                    }

                    var sprp = new SinglePrivateRegistryProcessor(filename);
                    sprp.Process();
                }
                catch (VSPrivateRegException ex)
                {
                    Console.Error.WriteLine($"ERROR: {ex.Message}");
                }
                catch (Win32Exception wex)
                {
                    Console.Error.WriteLine($"ERROR: {wex.Message}");
                }
            }
        }
    }
}