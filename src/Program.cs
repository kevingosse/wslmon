using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;

namespace wslmon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (var distribution in ListDistributions())
            {
                Console.WriteLine($"***** {distribution} *****");
                ListProcesses(distribution);
            }

            //var folders = Directory.GetDirectories(@"\\wsl.localhost\docker-desktop-data");

        }

        private static void ListProcesses(string distribution)
        {
            var root = $@"\\wsl$\{distribution}";

            if (!Directory.Exists(root))
            {
                Console.WriteLine($"Could not open share for distribution {distribution}");
                return;
            }

            var proc = $@"{root}\proc";

            if (!Directory.Exists(proc))
            {
                Console.WriteLine($"No /proc/ directory found for distribution {distribution}");
                return;
            }

            foreach (var directory in Directory.GetDirectories(proc))
            {
                InspectProcess(directory);
            }
        }

        private static void InspectProcess(string path)
        {
            if (!int.TryParse(path[(path.LastIndexOf('\\') + 1)..], out var pid))
            {
                return;
            }

            var cmdline = File.ReadAllText($@"{path}\cmdline");

            Console.WriteLine($"{pid} => {cmdline}");
        }

        private static IEnumerable<string> ListDistributions()
        {
            var lxss = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Lxss");

            if (lxss is null)
            {
                yield break;
            }

            foreach (var guid in lxss.GetSubKeyNames())
            {
                var distribution = lxss.OpenSubKey(guid);

                if (distribution is null)
                {
                    continue;
                }

                if (distribution.GetValue("DistributionName") is string name)
                {
                    yield return name;
                }
            }
        }
    }

}