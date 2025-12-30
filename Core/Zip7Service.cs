using System.Diagnostics;
using System.Linq;

namespace AutoBackupZipOneDrive.Core
{
    public static class Zip7Service
    {
        public static bool Create(string sevenZip, string outFile, string pwd, string[] files)
        {
            string args =
                "a -t7z \"" + outFile + "\" -p\"" + pwd + "\" -mhe=on -- " +
                string.Join(" ", files.Select(f => "\"" + f + "\""));

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = sevenZip,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process p = Process.Start(psi);
            if (p == null)
            {
               /* Logger.Write("Process.Start failed");*/
                return false;
            }

            string err = p.StandardError.ReadToEnd();
            p.WaitForExit();

/*            Logger.Write("CMD=" + sevenZip + " " + args);
            Logger.Write("ExitCode=" + p.ExitCode + " Err=" + err);*/

            return p.ExitCode == 0;
        }
    }
}
