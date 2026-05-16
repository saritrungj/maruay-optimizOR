using System;
using System.Collections.Generic;
using System.Linq;
using optimizOR.Models;

namespace optimizOR.Core
{
    public class TweakEngine
    {
        private readonly RegistryEditor _registryEditor;
        private readonly IniEditor _iniEditor;
        private readonly DebloatEditor _debloatEditor;
        private readonly Logger _logger;
        private readonly Dictionary<string, TweakDefinition> _tweaksById;
        private readonly HashSet<string> _appliedThisSession;

        public TweakEngine(RegistryEditor registryEditor, IniEditor iniEditor, DebloatEditor debloatEditor, Logger logger)
        {
            if (registryEditor == null)
            {
                throw new ArgumentNullException("registryEditor");
            }

            if (iniEditor == null)
            {
                throw new ArgumentNullException("iniEditor");
            }

            if (debloatEditor == null)
            {
                throw new ArgumentNullException("debloatEditor");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _registryEditor = registryEditor;
            _iniEditor = iniEditor;
            _debloatEditor = debloatEditor;
            _logger = logger;
            _tweaksById = TweakDefinitions.GetAll().ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
            _appliedThisSession = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void ApplyAll(IEnumerable<string> selectedTweakIds, IProgress<string> progress)
        {
            if (selectedTweakIds == null)
            {
                return;
            }

            foreach (string id in selectedTweakIds)
            {
                ApplyOne(id, progress);
            }
        }

        public bool ApplyOne(string tweakId, IProgress<string> progress)
        {
            try
            {
                TweakDefinition tweak;
                if (!_tweaksById.TryGetValue(tweakId, out tweak))
                {
                    _logger.Log("Unknown tweak id: " + tweakId, LogLevel.Warning);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report("Applying " + tweak.DisplayName);
                }
                bool success = ApplyDefinition(tweak);

                if (success)
                {
                    _appliedThisSession.Add(tweak.Id);
                    _logger.Log("Applied: " + tweak.DisplayName, LogLevel.Success);
                }
                else
                {
                    _logger.Log("Skipped or failed: " + tweak.DisplayName, LogLevel.Warning);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.Log("Apply failed for " + tweakId + ": " + ex.Message, LogLevel.Error);
                return false;
            }
        }

        public void RestoreAll(IProgress<string> progress)
        {
            foreach (string id in _appliedThisSession.ToList())
            {
                RestoreOne(id, progress);
            }
        }

        public bool RestoreOne(string tweakId, IProgress<string> progress)
        {
            try
            {
                TweakDefinition tweak;
                if (!_tweaksById.TryGetValue(tweakId, out tweak))
                {
                    _logger.Log("Unknown tweak id: " + tweakId, LogLevel.Warning);
                    return false;
                }

                if (progress != null)
                {
                    progress.Report("Restoring " + tweak.DisplayName);
                }
                bool success = RestoreDefinition(tweak);

                if (success)
                {
                    _appliedThisSession.Remove(tweak.Id);
                    _logger.Log("Restored: " + tweak.DisplayName, LogLevel.Success);
                }
                else
                {
                    _logger.Log("Restore skipped or failed: " + tweak.DisplayName, LogLevel.Warning);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.Log("Restore failed for " + tweakId + ": " + ex.Message, LogLevel.Error);
                return false;
            }
        }

        private bool ApplyDefinition(TweakDefinition tweak)
        {
            if (tweak.Type == TweakType.Registry)
            {
                bool ok = _registryEditor.ApplyTweak(tweak.Target, tweak.ValueName, tweak.TweakValue, tweak.ValueKind);
                foreach (RegistryValueDefinition extra in tweak.AdditionalRegistryValues)
                {
                    ok = _registryEditor.ApplyTweak(extra.KeyPath, extra.ValueName, extra.TweakValue, extra.ValueKind) && ok;
                }

                return ok;
            }

            if (tweak.Type == TweakType.Debloat)
            {
                return _debloatEditor.ApplyDebloat(tweak.DisplayName, tweak.PowerShellScript);
            }

            return _iniEditor.WriteValue(tweak.Target, tweak.IniSection, tweak.ValueName, Convert.ToString(tweak.TweakValue));
        }

        private bool RestoreDefinition(TweakDefinition tweak)
        {
            if (tweak.Type == TweakType.Registry)
            {
                bool ok = _registryEditor.RestoreDefault(tweak.Target, tweak.ValueName);
                foreach (RegistryValueDefinition extra in tweak.AdditionalRegistryValues)
                {
                    ok = _registryEditor.RestoreDefault(extra.KeyPath, extra.ValueName) && ok;
                }

                return ok;
            }

            if (tweak.Type == TweakType.Debloat)
            {
                _logger.Log("Debloat restore is not automatic. Reinstall removed apps from Microsoft Store if needed: " + tweak.DisplayName, LogLevel.Warning);
                return false;
            }

            return _iniEditor.RestoreFromBackup(tweak.Target);
        }
    }
}

