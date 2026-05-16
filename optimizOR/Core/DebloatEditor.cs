using System;
using System.Diagnostics;
using System.Text;
using optimizOR.Models;

namespace optimizOR.Core
{
    public class DebloatEditor
    {
        private readonly Logger _logger;

        public DebloatEditor(Logger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _logger = logger;
        }

        public bool ApplyDebloat(string displayName, string powerShellScript)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(powerShellScript))
                {
                    _logger.Log("Debloat script is empty: " + displayName, LogLevel.Warning);
                    return false;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -EncodedCommand " + EncodePowerShell(powerShellScript),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        _logger.Log("Unable to start PowerShell for: " + displayName, LogLevel.Error);
                        return false;
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        _logger.Log(output.Trim(), LogLevel.Info);
                    }

                    if (process.ExitCode == 0)
                    {
                        return true;
                    }

                    _logger.Log("Debloat failed for " + displayName + ": " + error.Trim(), LogLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Debloat failed for " + displayName + ": " + ex.Message, LogLevel.Error);
                return false;
            }
        }

        private static string EncodePowerShell(string script)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        }
    }
}

