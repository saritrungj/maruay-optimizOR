using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Microsoft.Win32;
using optimizOR.Models;

namespace optimizOR.Core
{
    public class RegistryEditor
    {
        private readonly Logger _logger;
        private readonly Dictionary<string, RegistryBackup> _backups;

        public RegistryEditor(Logger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _logger = logger;
            _backups = new Dictionary<string, RegistryBackup>(StringComparer.OrdinalIgnoreCase);
        }

        public bool ApplyTweak(string keyPath, string valueName, object value, RegistryValueKind kind)
        {
            try
            {
                BackupValue(keyPath, valueName);

                RegistryHive hive;
                string subKeyPath;
                if (!TrySplitPath(keyPath, out hive, out subKeyPath))
                {
                    _logger.Log("Invalid registry path: " + keyPath, LogLevel.Error);
                    return false;
                }

                using (RegistryKey root = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                using (RegistryKey key = root.CreateSubKey(subKeyPath))
                {
                    if (key == null)
                    {
                        _logger.Log("Unable to open registry key: " + keyPath, LogLevel.Error);
                        return false;
                    }

                    key.SetValue(valueName, value, kind);
                    return true;
                }
            }
            catch (SecurityException ex)
            {
                _logger.Log("Registry security error for " + keyPath + ": " + ex.Message, LogLevel.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Log("Registry access denied for " + keyPath + ": " + ex.Message, LogLevel.Error);
            }
            catch (IOException ex)
            {
                _logger.Log("Registry I/O error for " + keyPath + ": " + ex.Message, LogLevel.Error);
            }
            catch (Exception ex)
            {
                _logger.Log("Registry write failed for " + keyPath + ": " + ex.Message, LogLevel.Error);
            }

            return false;
        }

        public bool RestoreDefault(string keyPath, string valueName)
        {
            try
            {
                string backupKey = BuildBackupKey(keyPath, valueName);
                RegistryBackup backup;
                if (!_backups.TryGetValue(backupKey, out backup))
                {
                    _logger.Log("No registry backup found for " + backupKey, LogLevel.Warning);
                    return false;
                }

                RegistryHive hive;
                string subKeyPath;
                if (!TrySplitPath(keyPath, out hive, out subKeyPath))
                {
                    _logger.Log("Invalid registry path: " + keyPath, LogLevel.Error);
                    return false;
                }

                using (RegistryKey root = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                using (RegistryKey key = root.CreateSubKey(subKeyPath))
                {
                    if (key == null)
                    {
                        _logger.Log("Unable to open registry key for restore: " + keyPath, LogLevel.Error);
                        return false;
                    }

                    if (backup.Existed)
                    {
                        key.SetValue(valueName, backup.Value, backup.Kind);
                    }
                    else
                    {
                        key.DeleteValue(valueName, false);
                    }

                    return true;
                }
            }
            catch (SecurityException ex)
            {
                _logger.Log("Registry restore security error for " + keyPath + ": " + ex.Message, LogLevel.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Log("Registry restore access denied for " + keyPath + ": " + ex.Message, LogLevel.Error);
            }
            catch (IOException ex)
            {
                _logger.Log("Registry restore I/O error for " + keyPath + ": " + ex.Message, LogLevel.Error);
            }
            catch (Exception ex)
            {
                _logger.Log("Registry restore failed for " + keyPath + ": " + ex.Message, LogLevel.Error);
            }

            return false;
        }

        public void BackupValue(string keyPath, string valueName)
        {
            try
            {
                string backupKey = BuildBackupKey(keyPath, valueName);
                if (_backups.ContainsKey(backupKey))
                {
                    return;
                }

                RegistryHive hive;
                string subKeyPath;
                if (!TrySplitPath(keyPath, out hive, out subKeyPath))
                {
                    _logger.Log("Invalid registry path for backup: " + keyPath, LogLevel.Error);
                    return;
                }

                using (RegistryKey root = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                using (RegistryKey key = root.OpenSubKey(subKeyPath, false))
                {
                    if (key == null)
                    {
                        _backups[backupKey] = new RegistryBackup(false, null, RegistryValueKind.Unknown);
                        _logger.Log("Registry key missing before write; it will be created: " + keyPath, LogLevel.Warning);
                        return;
                    }

                    object value = key.GetValue(valueName, null);
                    if (value == null)
                    {
                        _backups[backupKey] = new RegistryBackup(false, null, RegistryValueKind.Unknown);
                    }
                    else
                    {
                        _backups[backupKey] = new RegistryBackup(true, value, key.GetValueKind(valueName));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Registry backup failed for " + keyPath + "\\" + valueName + ": " + ex.Message, LogLevel.Error);
            }
        }

        public object ReadValue(string keyPath, string valueName)
        {
            try
            {
                RegistryHive hive;
                string subKeyPath;
                if (!TrySplitPath(keyPath, out hive, out subKeyPath))
                {
                    return null;
                }

                using (RegistryKey root = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                using (RegistryKey key = root.OpenSubKey(subKeyPath, false))
                {
                    return key == null ? null : key.GetValue(valueName, null);
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Registry read failed for " + keyPath + "\\" + valueName + ": " + ex.Message, LogLevel.Error);
                return null;
            }
        }

        public bool KeyExists(string keyPath)
        {
            try
            {
                RegistryHive hive;
                string subKeyPath;
                if (!TrySplitPath(keyPath, out hive, out subKeyPath))
                {
                    return false;
                }

                using (RegistryKey root = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                using (RegistryKey key = root.OpenSubKey(subKeyPath, false))
                {
                    return key != null;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Registry key check failed for " + keyPath + ": " + ex.Message, LogLevel.Error);
                return false;
            }
        }

        private static string BuildBackupKey(string keyPath, string valueName)
        {
            return keyPath + "\\" + valueName;
        }

        private static bool TrySplitPath(string keyPath, out RegistryHive hive, out string subKeyPath)
        {
            hive = RegistryHive.LocalMachine;
            subKeyPath = string.Empty;

            if (string.IsNullOrWhiteSpace(keyPath))
            {
                return false;
            }

            int slash = keyPath.IndexOf('\\');
            string root = slash >= 0 ? keyPath.Substring(0, slash) : keyPath;
            subKeyPath = slash >= 0 ? keyPath.Substring(slash + 1) : string.Empty;

            if (root.Equals("HKLM", StringComparison.OrdinalIgnoreCase) ||
                root.Equals("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase))
            {
                hive = RegistryHive.LocalMachine;
                return true;
            }

            if (root.Equals("HKCU", StringComparison.OrdinalIgnoreCase) ||
                root.Equals("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
            {
                hive = RegistryHive.CurrentUser;
                return true;
            }

            return false;
        }

        private sealed class RegistryBackup
        {
            public RegistryBackup(bool existed, object value, RegistryValueKind kind)
            {
                Existed = existed;
                Value = value;
                Kind = kind;
            }

            public bool Existed { get; private set; }
            public object Value { get; private set; }
            public RegistryValueKind Kind { get; private set; }
        }
    }
}

