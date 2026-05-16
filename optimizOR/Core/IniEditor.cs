using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using optimizOR.Models;

namespace optimizOR.Core
{
    public class IniEditor
    {
        private readonly Logger _logger;
        private readonly HashSet<string> _backedUpFiles;

        public IniEditor(Logger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _logger = logger;
            _backedUpFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpString,
            string lpFileName);

        public string ReadValue(string filePath, string section, string key)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.Log("INI file missing: " + filePath, LogLevel.Warning);
                    return null;
                }

                StringBuilder buffer = new StringBuilder(1024);
                GetPrivateProfileString(section, key, string.Empty, buffer, (uint)buffer.Capacity, filePath);
                return buffer.ToString();
            }
            catch (IOException ex)
            {
                _logger.Log("INI read I/O error for " + filePath + ": " + ex.Message, LogLevel.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Log("INI read access denied for " + filePath + ": " + ex.Message, LogLevel.Error);
            }
            catch (Exception ex)
            {
                _logger.Log("INI read failed for " + filePath + ": " + ex.Message, LogLevel.Error);
            }

            return null;
        }

        public bool WriteValue(string filePath, string section, string key, string value)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.Log("INI file missing; skipping tweak: " + filePath, LogLevel.Warning);
                    return false;
                }

                if (!BackupFile(filePath))
                {
                    return false;
                }

                bool ok = WritePrivateProfileString(section, key, value, filePath);
                if (!ok)
                {
                    _logger.Log("Windows rejected INI write: " + filePath + " [" + section + "] " + key, LogLevel.Error);
                }

                return ok;
            }
            catch (IOException ex)
            {
                _logger.Log("INI write I/O error for " + filePath + ": " + ex.Message, LogLevel.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Log("INI write access denied for " + filePath + ": " + ex.Message, LogLevel.Error);
            }
            catch (Exception ex)
            {
                _logger.Log("INI write failed for " + filePath + ": " + ex.Message, LogLevel.Error);
            }

            return false;
        }

        public bool BackupFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.Log("INI file missing; backup skipped: " + filePath, LogLevel.Warning);
                    return false;
                }

                string fullPath = Path.GetFullPath(filePath);
                if (_backedUpFiles.Contains(fullPath))
                {
                    return true;
                }

                string backupPath = filePath + ".bak";
                File.Copy(filePath, backupPath, true);
                _backedUpFiles.Add(fullPath);
                _logger.Log("INI backup created: " + backupPath, LogLevel.Info);
                return true;
            }
            catch (IOException ex)
            {
                _logger.Log("INI backup I/O error for " + filePath + ": " + ex.Message, LogLevel.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Log("INI backup access denied for " + filePath + ": " + ex.Message, LogLevel.Error);
            }
            catch (Exception ex)
            {
                _logger.Log("INI backup failed for " + filePath + ": " + ex.Message, LogLevel.Error);
            }

            return false;
        }

        public bool RestoreFromBackup(string filePath)
        {
            try
            {
                string backupPath = filePath + ".bak";
                if (!File.Exists(backupPath))
                {
                    _logger.Log("INI backup missing; restore skipped: " + backupPath, LogLevel.Warning);
                    return false;
                }

                File.Copy(backupPath, filePath, true);
                return true;
            }
            catch (IOException ex)
            {
                _logger.Log("INI restore I/O error for " + filePath + ": " + ex.Message, LogLevel.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Log("INI restore access denied for " + filePath + ": " + ex.Message, LogLevel.Error);
            }
            catch (Exception ex)
            {
                _logger.Log("INI restore failed for " + filePath + ": " + ex.Message, LogLevel.Error);
            }

            return false;
        }

        public bool SectionExists(string filePath, string section)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.Log("INI file missing: " + filePath, LogLevel.Warning);
                    return false;
                }

                StringBuilder buffer = new StringBuilder(4096);
                uint count = GetPrivateProfileString(section, null, string.Empty, buffer, (uint)buffer.Capacity, filePath);
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.Log("INI section check failed for " + filePath + ": " + ex.Message, LogLevel.Error);
                return false;
            }
        }
    }
}

