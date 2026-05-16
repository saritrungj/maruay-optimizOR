using System;
using System.Collections.Generic;
using Microsoft.Win32;
using optimizOR.Models;

namespace optimizOR.Core
{
    public static class TweakDefinitions
    {
        private static readonly IReadOnlyList<TweakDefinition> Tweaks = BuildTweaks();

        public static IReadOnlyList<TweakDefinition> GetAll()
        {
            return Tweaks;
        }

        private static IReadOnlyList<TweakDefinition> BuildTweaks()
        {
            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string systemIni = System.IO.Path.Combine(windows, "SYSTEM.INI");
            string winIni = System.IO.Path.Combine(windows, "WIN.INI");

            List<TweakDefinition> tweaks = new List<TweakDefinition>
            {
                Registry("WinPriority", "Optimize Win32PrioritySeparation (0x2E)",
                    "Boosts foreground process scheduling for games.",
                    @"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl",
                    "Win32PrioritySeparation", 0x2E, 0x02, RegistryValueKind.DWord, "Priority"),

                Registry("MMCSS_GamePriority", "Boost MMCSS Game Task Priority",
                    "Sets the Games task priority to 6.",
                    @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                    "Priority", 6, 2, RegistryValueKind.DWord, "Priority"),

                Registry("MMCSS_HighCategory", "Set MMCSS Scheduling Category: High",
                    "Raises the Games task scheduling category.",
                    @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                    "Scheduling Category", "High", "Medium", RegistryValueKind.String, "Priority"),

                Registry("MMCSS_GPUPriority", "Set MMCSS GPU Priority (8)",
                    "Sets the Games task GPU priority to 8.",
                    @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                    "GPU Priority", 8, 2, RegistryValueKind.DWord, "Priority"),

                Registry("MMCSS_Responsiveness", "Set SystemResponsiveness to 0",
                    "Reduces multimedia scheduler reserved CPU responsiveness.",
                    @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    "SystemResponsiveness", 0, 20, RegistryValueKind.DWord, "Priority"),

                Registry("NetworkThrottle", "Disable Network Throttling Index",
                    "Disables multimedia network throttling.",
                    @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF), 10, RegistryValueKind.DWord, "Latency"),

                Registry("ZeroTimeSlice", "Zero TimeSlice (IRQ8 Priority Boost)",
                    "Enables IRQ8 priority boost.",
                    @"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl",
                    "IRQ8Priority", 1, 0, RegistryValueKind.DWord, "Latency"),

                Registry("DynamicTick", "Set RealTimeIsUniversal (Timer Fix)",
                    "Applies the timer-related RealTimeIsUniversal value.",
                    @"HKLM\SYSTEM\CurrentControlSet\Control\TimeZoneInformation",
                    "RealTimeIsUniversal", 1, 0, RegistryValueKind.DWord, "Latency"),

                Registry("DisablePaging", "Disable Paging Executive (Keep in RAM)",
                    "Keeps kernel-mode drivers and system code resident in memory.",
                    @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    "DisablePagingExecutive", 1, 0, RegistryValueKind.DWord, "Memory"),

                Registry("LargeSystemCache", "Optimize Large System Cache (Gaming)",
                    "Sets LargeSystemCache to the gaming-safe default.",
                    @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    "LargeSystemCache", 0, 0, RegistryValueKind.DWord, "Memory"),

                Mitigations(),

                Ini("IniIRQ", "Inject IRQ9=4 in [386Enh] (SYSTEM.INI)",
                    "Adds IRQ9=4 to SYSTEM.INI.",
                    systemIni, "386Enh", "IRQ9", "4", null, "System.INI"),

                Ini("IniMinSPs", "Set MinSPs=4 in [386Enh] (SYSTEM.INI)",
                    "Adds MinSPs=4 to SYSTEM.INI.",
                    systemIni, "386Enh", "MinSPs", "4", null, "System.INI"),

                Ini("IniWinLoad", "Clear Win.INI Load= Entry",
                    "Clears the Windows load entry.",
                    winIni, "windows", "load", string.Empty, null, "Win.INI"),

                Debloat("DebloatConsumerApps", "Remove consumer bundled apps",
                    "Removes common optional consumer Appx packages for the current user and from provisioning for new users. Keeps Store, Security, Edge, Terminal, Photos, Calculator, Notepad, codecs, and framework packages.",
                    ConsumerAppsScript()),

                Debloat("DebloatXbox", "Remove Xbox companion apps",
                    "Removes optional Xbox Game Bar and Xbox companion packages. Skip this if you use Game Bar recording, Xbox services, or PC Game Pass overlays.",
                    AppxRemovalScript(new[]
                    {
                        "Microsoft.XboxApp",
                        "Microsoft.Xbox.TCUI",
                        "Microsoft.XboxGameOverlay",
                        "Microsoft.XboxGamingOverlay",
                        "Microsoft.XboxIdentityProvider",
                        "Microsoft.XboxSpeechToTextOverlay",
                        "Microsoft.GamingApp"
                    })),

                Debloat("DebloatTeamsChat", "Remove consumer Teams and Chat",
                    "Removes consumer Teams/Chat packages and disables Chat taskbar integration where supported. This does not remove business Microsoft Teams installed outside Appx.",
                    TeamsChatScript()),

                Debloat("DebloatWidgets", "Disable Widgets and News",
                    "Disables Windows 10 News and Interests plus Windows 11 Widgets policies/taskbar entry where supported.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Dsh' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Dsh' -Name 'AllowNewsAndInterests' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'TaskbarDa' -Value 0 -PropertyType DWord -Force | Out-Null"
                    })),

                Debloat("DebloatSuggestions", "Disable ads, tips, and suggestions",
                    "Disables common Windows consumer suggestions, silent app installs, Start suggestions, lock screen tips, and Explorer search box web suggestions.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Force | Out-Null",
                        @"'ContentDeliveryAllowed','OemPreInstalledAppsEnabled','PreInstalledAppsEnabled','PreInstalledAppsEverEnabled','SilentInstalledAppsEnabled','SubscribedContent-338388Enabled','SubscribedContent-338389Enabled','SubscribedContent-338393Enabled','SubscribedContent-353694Enabled','SubscribedContent-353696Enabled','SystemPaneSuggestionsEnabled' | ForEach-Object { New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Name $_ -Value 0 -PropertyType DWord -Force | Out-Null }",
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'Start_TrackProgs' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Name 'DisableSearchBoxSuggestions' -Value 1 -PropertyType DWord -Force | Out-Null"
                    })),

                Debloat("DebloatOneDriveStartup", "Disable OneDrive startup",
                    "Disables OneDrive auto-start entries without uninstalling OneDrive or deleting user files.",
                    RegistryPolicyScript(new[]
                    {
                        @"Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'OneDrive' -ErrorAction SilentlyContinue",
                        @"Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'OneDriveSetup' -ErrorAction SilentlyContinue"
                    }))
            };

            return tweaks.AsReadOnly();
        }

        private static TweakDefinition Registry(
            string id,
            string displayName,
            string description,
            string target,
            string valueName,
            object tweakValue,
            object defaultValue,
            RegistryValueKind kind,
            string category)
        {
            return new TweakDefinition
            {
                Id = id,
                DisplayName = displayName,
                Description = description,
                Type = TweakType.Registry,
                Target = target,
                ValueName = valueName,
                TweakValue = tweakValue,
                DefaultValue = defaultValue,
                ValueKind = kind,
                Category = category
            };
        }

        private static TweakDefinition Ini(
            string id,
            string displayName,
            string description,
            string target,
            string section,
            string key,
            string tweakValue,
            object defaultValue,
            string category)
        {
            return new TweakDefinition
            {
                Id = id,
                DisplayName = displayName,
                Description = description,
                Type = TweakType.IniFile,
                Target = target,
                IniSection = section,
                ValueName = key,
                TweakValue = tweakValue,
                DefaultValue = defaultValue,
                Category = category
            };
        }

        private static TweakDefinition Mitigations()
        {
            TweakDefinition definition = Registry("Mitigations", "Disable OS Mitigations (Spectre/Meltdown)",
                "Disables selected speculative execution mitigations. Use only on trusted systems.",
                @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                "FeatureSettingsOverride", 3, 0, RegistryValueKind.DWord, "Memory");

            definition.AdditionalRegistryValues.Add(new RegistryValueDefinition
            {
                KeyPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                ValueName = "FeatureSettingsOverrideMask",
                TweakValue = 3,
                DefaultValue = 0,
                ValueKind = RegistryValueKind.DWord
            });

            return definition;
        }

        private static TweakDefinition Debloat(string id, string displayName, string description, string script)
        {
            return new TweakDefinition
            {
                Id = id,
                DisplayName = displayName,
                Description = description,
                Type = TweakType.Debloat,
                Category = "Debloat",
                PowerShellScript = script
            };
        }

        private static string ConsumerAppsScript()
        {
            return AppxRemovalScript(new[]
            {
                "Microsoft.549981C3F5F10",
                "Microsoft.BingNews",
                "Microsoft.BingWeather",
                "Microsoft.GetHelp",
                "Microsoft.Getstarted",
                "Microsoft.Microsoft3DViewer",
                "Microsoft.MicrosoftOfficeHub",
                "Microsoft.MicrosoftSolitaireCollection",
                "Microsoft.MixedReality.Portal",
                "Microsoft.Office.OneNote",
                "Microsoft.People",
                "Microsoft.PowerAutomateDesktop",
                "Microsoft.SkypeApp",
                "Microsoft.Todos",
                "Microsoft.Wallet",
                "Microsoft.WindowsAlarms",
                "Microsoft.WindowsFeedbackHub",
                "Microsoft.WindowsMaps",
                "Microsoft.WindowsSoundRecorder",
                "Microsoft.YourPhone",
                "Microsoft.ZuneMusic",
                "Microsoft.ZuneVideo",
                "Clipchamp.Clipchamp",
                "MicrosoftCorporationII.QuickAssist",
                "MicrosoftCorporationII.MicrosoftFamily"
            });
        }

        private static string TeamsChatScript()
        {
            return AppxRemovalScript(new[] { "MicrosoftTeams", "MSTeams" }) + "; " +
                RegistryPolicyScript(new[]
                {
                    @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Force | Out-Null",
                    @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'TaskbarMn' -Value 0 -PropertyType DWord -Force | Out-Null",
                    @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Chat' -Force | Out-Null",
                    @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Chat' -Name 'ChatIcon' -Value 3 -PropertyType DWord -Force | Out-Null"
                });
        }

        private static string AppxRemovalScript(IEnumerable<string> packageNames)
        {
            string joined = "'" + string.Join("','", packageNames) + "'";
            return "$packages = @(" + joined + "); " +
                "foreach ($package in $packages) { " +
                "Get-AppxPackage -Name $package -AllUsers -ErrorAction SilentlyContinue | ForEach-Object { try { Remove-AppxPackage -Package $_.PackageFullName -AllUsers -ErrorAction Stop } catch { Remove-AppxPackage -Package $_.PackageFullName -ErrorAction SilentlyContinue } }; " +
                "Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -eq $package } | ForEach-Object { Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName -ErrorAction SilentlyContinue | Out-Null }; " +
                "}; Write-Output ('Debloat Appx pass completed: ' + ($packages -join ', '))";
        }

        private static string RegistryPolicyScript(IEnumerable<string> commands)
        {
            return string.Join("; ", commands) + "; Write-Output 'Debloat policy pass completed.'";
        }
    }
}

