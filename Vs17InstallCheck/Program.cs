using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vs17InstallCheck
{
    class Program
    {
        const string vsDirPattern = "1[56789].[0-9]+_[a-z0-9]";
        const string vsProgDataDirPattern = "[a-z0-9]+";

        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("User local appdata entries for Visual Studio");
                var query = new SelectQuery("Win32_UserAccount");
                var searcher = new ManagementObjectSearcher(query);
                foreach (var mobject in searcher.Get())
                {
                    var userPath = $@"c:\users\{mobject["Name"]}\appdata\local\microsoft\visualstudio\";
                    if (Directory.Exists(userPath))
                    {
                        var directories = Directory.GetDirectories(userPath);
                        var nameList = directories.Where(x => Regex.Match(new DirectoryInfo(x).Name, vsDirPattern, RegexOptions.IgnoreCase).Success);
                        foreach (var entry in nameList)
                        {
                            var path = Path.Combine(entry, "privateregistry.bin");
                            var reportName = File.Exists(path) ? "privateregistry.bin" : "";
                            Console.WriteLine($"    {entry} {reportName}");
                        }
                    }
                }

                var vsProgramDataFolder = @"C:\ProgramData\Microsoft\VisualStudio\Packages\_Instances";

                Console.WriteLine();
                Console.WriteLine(@"c:\programdata entries representing VS instances:");
                var dirs = Directory.GetDirectories(vsProgramDataFolder);
                var instanceDirs = dirs.Where(x => Regex.Match(new DirectoryInfo(x).Name, vsProgDataDirPattern, RegexOptions.IgnoreCase).Success);
                foreach (var dir in instanceDirs)
                {
                    Console.WriteLine($"    {dir}");
                }

                Console.WriteLine();
                Console.WriteLine(@"c:\programdata details for VS instances (from the state.json file):");
                foreach (var dir in instanceDirs)
                {
                    var stateFile = Path.Combine(dir, "state.json");
                    Console.WriteLine($"    {stateFile}");
                    if (File.Exists(stateFile))
                    {
                        using (StreamReader reader = File.OpenText(stateFile))
                        {
                            JObject vsJsonInfo = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                            var topLevelInfoObjects = new[] { "installationPath", "installDate", "updateDate", "layoutPath", "installationVersion" };
                            foreach (var tlObject in topLevelInfoObjects)
                            {
                                var value = vsJsonInfo[tlObject] ?? $"NOT FOUND IN {stateFile}";
                                Console.WriteLine($"        {tlObject} - {value}");
                            }
                        }
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
                var progname = Path.GetFileNameWithoutExtension(fullname);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
