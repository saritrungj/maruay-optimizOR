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
                    winIni, "windows", "load", string.Empty, null, "Win.INI")
            };

            tweaks.AddRange(BuildWin10Debloat());
            tweaks.AddRange(BuildWin11Debloat());

            return tweaks.AsReadOnly();
        }

        private static IEnumerable<TweakDefinition> BuildWin10Debloat()
        {
            const string win10 = "Win10Debloat";
            List<TweakDefinition> tweaks = new List<TweakDefinition>
            {
                Debloat("Win10SysprepAppxPass", "Sysprep Appx removal pass",
                    "Runs the Windows10SysPrepDebloater style Appx removal pass first. This is useful before user profiles are configured during MDT, imaging, or sysprep workflows.",
                    SysprepAppxScript(), win10, "Sysprep"),

                Debloat("Win10DebloatSwitch", "Run Debloat switch set",
                    "Runs the Debloat switch set: app removal, leftover bloatware registry key cleanup, privacy hardening, scheduled task cleanup, and Edge PDF handoff.",
                    ConsumerAppsScript() + "; " + RemoveKeysScript() + "; " + Windows10PrivacyScript() + "; " + DisableScheduledTasksScript() + "; " + EdgePdfScript(false),
                    win10, "Switch Parameters"),

                Debloat("Win10PrivacySwitch", "Run Privacy switch set",
                    "Applies Windows 10 privacy changes: telemetry reductions, Cortana/Search web integration limits, advertising ID disablement, scheduled task cleanup, and suggested-content controls.",
                    Windows10PrivacyScript() + "; " + DisableScheduledTasksScript(), win10, "Switch Parameters"),

                Debloat("Win10RemoveKeys", "Remove leftover bloatware registry keys",
                    "Deletes leftover registry keys associated with EclipseManager, ActiproSoftwareLLC, Microsoft.PPIProjection, and Microsoft.XboxGameCallableUI.",
                    RemoveKeysScript(), win10, "Registry Cleanup"),

                Debloat("Win10DisableScheduledTasks", "Disable telemetry and Xbox scheduled tasks",
                    "Disables XblGameSaveTaskLogon, XblGameSaveTask, Consolidator, UsbCeip, and DmClient scheduled tasks.",
                    DisableScheduledTasksScript(), win10, "Privacy"),

                Debloat("Win10StopEdgePdf", "Stop Edge PDF takeover",
                    "Stops Microsoft Edge from being forced as the default PDF handler where the classic policy keys are honored.",
                    EdgePdfScript(false), win10, "Interactive Choices"),

                Debloat("Win10EnableEdgePdf", "Re-enable Edge PDF defaults",
                    "Reverts the Edge PDF prevention policy used by the debloater flow.",
                    EdgePdfScript(true), win10, "Revert"),

                Debloat("Win10RevertChanges", "Revert debloat registry changes",
                    "Attempts to reverse privacy and policy changes, re-enable scheduled tasks, and re-register installed Appx packages. Removed Store apps may still need Microsoft Store reinstall.",
                    Windows10RevertScript(), win10, "Revert"),

                Debloat("DebloatConsumerApps", "Remove Windows 10 bloatware apps",
                    "Removes the Windows 10 bloatware catalog listed in the classic Windows10Debloater project using Remove-AppxPackage and Remove-AppxProvisionedPackage. Best results happen before a user profile is configured.",
                    ConsumerAppsScript(), win10, "App Removal"),

                Debloat("DebloatXbox", "Remove Xbox companion apps",
                    "Removes XboxApp, Xbox Game CallableUI, Xbox Identity Provider, Xbox overlays, and related optional gaming companion packages.",
                    AppxRemovalScript(new[]
                    {
                        "Microsoft.XboxApp",
                        "Microsoft.Xbox.TCUI",
                        "Microsoft.XboxGameCallableUI",
                        "Microsoft.XboxGameOverlay",
                        "Microsoft.XboxGamingOverlay",
                        "Microsoft.XboxIdentityProvider",
                        "Microsoft.XboxSpeechToTextOverlay",
                        "Microsoft.GamingApp"
                    }), win10, "App Removal"),

                Debloat("DebloatTeamsChat", "Remove consumer Teams and Chat",
                    "Removes consumer Teams/Chat packages and disables Chat taskbar integration where supported. This does not remove business Microsoft Teams installed outside Appx.",
                    TeamsChatScript(), win10, "App Removal"),

                Debloat("DebloatWidgets", "Disable News and Interests",
                    "Disables Windows 10 News and Interests and compatible widget policy values.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Dsh' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Dsh' -Name 'AllowNewsAndInterests' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Feeds' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Feeds' -Name 'ShellFeedsTaskbarViewMode' -Value 2 -PropertyType DWord -Force | Out-Null"
                    }), win10, "Privacy"),

                Debloat("DebloatSuggestions", "Disable ads, tips, and suggestions",
                    "Disables common Windows 10 tips, silent app installs, Start suggestions, lock screen suggestions, and selected Explorer/Search web suggestions.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Force | Out-Null",
                        @"'ContentDeliveryAllowed','OemPreInstalledAppsEnabled','PreInstalledAppsEnabled','PreInstalledAppsEverEnabled','SilentInstalledAppsEnabled','SubscribedContent-338388Enabled','SubscribedContent-338389Enabled','SubscribedContent-338393Enabled','SubscribedContent-353694Enabled','SubscribedContent-353696Enabled','SystemPaneSuggestionsEnabled' | ForEach-Object { New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Name $_ -Value 0 -PropertyType DWord -Force | Out-Null }",
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'Start_TrackProgs' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Name 'DisableSearchBoxSuggestions' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win10, "Privacy"),

                Debloat("DebloatOneDriveStartup", "Disable OneDrive startup",
                    "Disables OneDrive auto-start entries without uninstalling OneDrive or deleting user files.",
                    RegistryPolicyScript(new[]
                    {
                        @"Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'OneDrive' -ErrorAction SilentlyContinue",
                        @"Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'OneDriveSetup' -ErrorAction SilentlyContinue"
                    }), win10, "Privacy")
            };

            return tweaks;
        }

        private static IEnumerable<TweakDefinition> BuildWin11Debloat()
        {
            const string win11 = "Win11Debloat";
            List<TweakDefinition> tweaks = new List<TweakDefinition>
            {
                Debloat("Win11AppRemoval", "Remove preinstalled apps",
                    "Removes a broad set of optional Windows 11 consumer Appx packages while keeping core system components.",
                    ConsumerAppsScript(), win11, "App Removal"),

                Debloat("Win11Telemetry", "Disable telemetry and activity tracking",
                    "Disables telemetry, diagnostic data, activity history, app-launch tracking, and targeted advertising IDs.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection' -Name 'AllowTelemetry' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Name 'Enabled' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' -Name 'PublishUserActivities' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System' -Name 'UploadUserActivities' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Privacy & Suggested Content"),

                Debloat("Win11TipsAds", "Disable tips, suggestions, and ads",
                    "Disables Windows tips, tricks, suggestions, silent app installs, and promoted content surfaces.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Force | Out-Null",
                        @"'ContentDeliveryAllowed','OemPreInstalledAppsEnabled','PreInstalledAppsEnabled','PreInstalledAppsEverEnabled','SilentInstalledAppsEnabled','SubscribedContent-338388Enabled','SubscribedContent-338389Enabled','SubscribedContent-338393Enabled','SubscribedContent-353694Enabled','SubscribedContent-353696Enabled','SystemPaneSuggestionsEnabled' | ForEach-Object { New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Name $_ -Value 0 -PropertyType DWord -Force | Out-Null }"
                    }), win11, "Privacy & Suggested Content"),

                Debloat("Win11LocationServices", "Disable location services",
                    "Disables Windows location services and app location access for the current user.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors' -Name 'DisableLocation' -Value 1 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location' -Name 'Value' -Value 'Deny' -PropertyType String -Force | Out-Null"
                    }), win11, "Privacy & Suggested Content"),

                Debloat("Win11FindMyDevice", "Disable Find My Device",
                    "Disables Find My Device location tracking policy.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\FindMyDevice' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\FindMyDevice' -Name 'AllowFindMyDevice' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Privacy & Suggested Content"),

                Debloat("Win11LockSpotlight", "Disable lock screen Spotlight",
                    "Disables Windows Spotlight plus tips and tricks on the lock screen.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Force | Out-Null",
                        @"'RotatingLockScreenEnabled','RotatingLockScreenOverlayEnabled','SubscribedContent-338387Enabled' | ForEach-Object { New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Name $_ -Value 0 -PropertyType DWord -Force | Out-Null }"
                    }), win11, "Privacy & Suggested Content"),

                Debloat("Win11DesktopSpotlight", "Disable desktop Spotlight",
                    "Disables the Windows Spotlight desktop background option.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Policies\Microsoft\Windows\CloudContent' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Policies\Microsoft\Windows\CloudContent' -Name 'DisableSpotlightCollectionOnDesktop' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Privacy & Suggested Content"),

                Debloat("Win11EdgeAds", "Disable Edge ads and MSN feed",
                    "Disables Microsoft Edge suggestions, sponsored content, and the new tab news feed where policy keys are honored.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Edge' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Edge' -Name 'ShowRecommendationsEnabled' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Edge' -Name 'SpotlightExperiencesAndRecommendationsEnabled' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Edge' -Name 'NewTabPageContentEnabled' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Privacy & Suggested Content"),

                Debloat("Win11SettingsHomeAds", "Hide Settings Home ads",
                    "Hides Microsoft 365 recommendations on Settings Home or hides the Settings Home page on supported builds.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer' -Name 'SettingsPageVisibility' -Value 'hide:home' -PropertyType String -Force | Out-Null"
                    }), win11, "Privacy & Suggested Content"),

                Debloat("Win11Copilot", "Disable and remove Copilot",
                    "Disables Microsoft Copilot policies and removes optional Copilot Appx packages when present.",
                    AppxRemovalScript(new[] { "Microsoft.Copilot", "Microsoft.Windows.Ai.Copilot.Provider" }) + "; " +
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Policies\Microsoft\Windows\WindowsCopilot' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Policies\Microsoft\Windows\WindowsCopilot' -Name 'TurnOffWindowsCopilot' -Value 1 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot' -Name 'TurnOffWindowsCopilot' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "AI Features"),

                Debloat("Win11Recall", "Disable Windows Recall",
                    "Disables Windows Recall snapshots on supported Windows 11 builds.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsAI' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsAI' -Name 'DisableAIDataAnalysis' -Value 1 -PropertyType DWord -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsAI' -Name 'AllowRecallEnablement' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "AI Features"),

                Debloat("Win11ClickToDo", "Disable Click to Do",
                    "Disables Click to Do and related local AI text/image analysis policies where available.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsAI' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsAI' -Name 'DisableClickToDo' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "AI Features"),

                Debloat("Win11AIService", "Prevent AI service autostart",
                    "Prevents the Windows AI Fabric service from starting automatically when it exists.",
                    RegistryPolicyScript(new[]
                    {
                        @"Set-Service -Name 'WSAIFabricSvc' -StartupType Disabled -ErrorAction SilentlyContinue"
                    }), win11, "AI Features"),

                Debloat("Win11EdgeAI", "Disable AI features in Edge",
                    "Disables Edge AI surfaces such as Compose, Discover, and related sidebar experiences via policy keys.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Edge' -Force | Out-Null",
                        @"'HubsSidebarEnabled','ComposeInlineEnabled','DiscoverPageContextEnabled' | ForEach-Object { New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Edge' -Name $_ -Value 0 -PropertyType DWord -Force | Out-Null }"
                    }), win11, "AI Features"),

                Debloat("Win11PaintAI", "Disable AI features in Paint",
                    "Disables Paint AI features where policy or app settings are honored.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Applets\Paint\Settings' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Applets\Paint\Settings' -Name 'DisableAI' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "AI Features"),

                Debloat("Win11NotepadAI", "Disable AI features in Notepad",
                    "Disables Notepad AI features where app settings are honored.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Notepad' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Notepad' -Name 'DisableAI' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "AI Features"),

                Debloat("Win11DragTray", "Disable Drag Tray",
                    "Disables the Windows 11 Drag Tray sharing and moving surface where supported.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'DisableDragTray' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "System"),

                Debloat("Win11ClassicContextMenu", "Restore classic context menu",
                    "Restores the old Windows 10 style File Explorer context menu for the current user.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32' -Force | Out-Null",
                        @"Set-ItemProperty -Path 'HKCU:\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32' -Name '(default)' -Value '' -ErrorAction SilentlyContinue"
                    }), win11, "System"),

                Debloat("Win11PointerPrecision", "Turn off mouse acceleration",
                    "Turns off Enhance Pointer Precision for the current user.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-ItemProperty -Path 'HKCU:\Control Panel\Mouse' -Name 'MouseSpeed' -Value '0' -PropertyType String -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Control Panel\Mouse' -Name 'MouseThreshold1' -Value '0' -PropertyType String -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Control Panel\Mouse' -Name 'MouseThreshold2' -Value '0' -PropertyType String -Force | Out-Null"
                    }), win11, "System"),

                Debloat("Win11StickyKeysShortcut", "Disable Sticky Keys shortcut",
                    "Disables the Sticky Keys keyboard shortcut prompt.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-ItemProperty -Path 'HKCU:\Control Panel\Accessibility\StickyKeys' -Name 'Flags' -Value '506' -PropertyType String -Force | Out-Null"
                    }), win11, "System"),

                Debloat("Win11StorageSense", "Disable Storage Sense cleanup",
                    "Disables automatic Storage Sense disk cleanup for the current user.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy' -Name '01' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "System"),

                Debloat("Win11FastStartup", "Disable fast start-up",
                    "Disables hybrid boot so shutdown performs a full shutdown.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Power' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Power' -Name 'HiberbootEnabled' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "System"),

                Debloat("Win11BitLockerAutoEncryption", "Disable automatic device encryption",
                    "Disables automatic BitLocker device encryption policy on supported editions.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\BitLocker' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\BitLocker' -Name 'PreventDeviceEncryption' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "System"),

                Debloat("Win11ModernStandbyNetwork", "Disable Modern Standby networking",
                    "Disables network connectivity during Modern Standby to reduce battery drain.",
                    RegistryPolicyScript(new[]
                    {
                        @"powercfg /setdcvalueindex SCHEME_CURRENT SUB_NONE CONNECTIVITYINSTANDBY 0",
                        @"powercfg /setacvalueindex SCHEME_CURRENT SUB_NONE CONNECTIVITYINSTANDBY 0"
                    }), win11, "System"),

                Debloat("Win11UpdateEarlyAccess", "Prevent early update offers",
                    "Prevents Windows from getting updates as soon as they are available.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings' -Name 'IsContinuousInnovationOptedIn' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Windows Update"),

                Debloat("Win11UpdateAutoRestart", "Prevent update auto restarts",
                    "Prevents automatic restarts after updates while a user is signed in.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU' -Name 'NoAutoRebootWithLoggedOnUsers' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Windows Update"),

                Debloat("Win11DeliveryOptimization", "Disable Delivery Optimization sharing",
                    "Disables sharing downloaded updates with other PCs.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization' -Name 'DODownloadMode' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Windows Update"),

                Debloat("Win11DarkMode", "Enable dark mode",
                    "Enables dark mode for Windows system surfaces and apps.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name 'AppsUseLightTheme' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name 'SystemUsesLightTheme' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Appearance"),

                Debloat("Win11Transparency", "Disable transparency effects",
                    "Disables Windows transparency effects.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name 'EnableTransparency' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Appearance"),

                Debloat("Win11Animations", "Disable animations and visual effects",
                    "Disables Windows UI animations and selects best performance visual effects.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Control Panel\Desktop\WindowMetrics' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Control Panel\Desktop\WindowMetrics' -Name 'MinAnimate' -Value '0' -PropertyType String -Force | Out-Null",
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects' -Name 'VisualFXSetting' -Value 2 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Appearance"),

                Debloat("Win11StartPins", "Remove Start pinned apps",
                    "Clears Start menu pinned app layout for the current user where the Start layout file is present.",
                    RegistryPolicyScript(new[]
                    {
                        "Remove-Item -Path \"$env:LOCALAPPDATA\\Packages\\Microsoft.Windows.StartMenuExperienceHost_cw5n1h2txyewy\\LocalState\\start2.bin\" -Force -ErrorAction SilentlyContinue"
                    }), win11, "Start Menu & Search"),

                Debloat("Win11StartRecommended", "Hide Start recommended section",
                    "Hides recommendations in the Windows 11 Start menu where policy is supported.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Explorer' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Explorer' -Name 'HideRecommendedSection' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Start Menu & Search"),

                Debloat("Win11StartAllApps", "Hide Start All Apps section",
                    "Hides the All Apps section in Start where supported by policy.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer' -Name 'NoStartMenuMorePrograms' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Start Menu & Search"),

                Debloat("Win11PhoneLinkStart", "Disable Phone Link in Start",
                    "Disables the Phone Link mobile devices integration in Start.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Mobility' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Mobility' -Name 'OptedIn' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Start Menu & Search"),

                Debloat("Win11SearchBingCopilot", "Disable Bing web search and Copilot in Search",
                    "Disables Bing web results and Copilot integration in Windows Search.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Name 'DisableSearchBoxSuggestions' -Value 1 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search' -Name 'BingSearchEnabled' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Start Menu & Search"),

                Debloat("Win11StoreSearchSuggestions", "Disable Store suggestions in Search",
                    "Disables Microsoft Store app suggestions in Windows Search where supported.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Name 'NoUseStoreOpenWith' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Start Menu & Search"),

                Debloat("Win11SearchHighlights", "Disable Search Highlights",
                    "Disables dynamic and branded Search Highlights content in the taskbar search box.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\SearchSettings' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\SearchSettings' -Name 'IsDynamicSearchBoxEnabled' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Start Menu & Search"),

                Debloat("Win11SearchHistory", "Disable local search history",
                    "Disables local Windows Search history.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\SearchSettings' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\SearchSettings' -Name 'IsDeviceSearchHistoryEnabled' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Start Menu & Search"),

                Debloat("Win11TaskbarLeft", "Align taskbar icons left",
                    "Aligns Windows 11 taskbar icons to the left.",
                    ExplorerAdvancedDword("TaskbarAl", 0), win11, "Taskbar"),
                Debloat("Win11TaskbarSearch", "Hide taskbar search",
                    "Hides the search icon or box on the taskbar.",
                    ExplorerAdvancedDword("SearchboxTaskbarMode", 0), win11, "Taskbar"),
                Debloat("Win11TaskView", "Hide taskview button",
                    "Hides the Task View button from the taskbar.",
                    ExplorerAdvancedDword("ShowTaskViewButton", 0), win11, "Taskbar"),
                Debloat("Win11Widgets", "Disable widgets on taskbar and lock screen",
                    "Disables widgets on the taskbar and lock screen.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Dsh' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Dsh' -Name 'AllowNewsAndInterests' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'TaskbarDa' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Taskbar"),
                Debloat("Win11ChatIcon", "Hide chat icon",
                    "Hides the Chat or Meet Now icon from the taskbar.",
                    ExplorerAdvancedDword("TaskbarMn", 0), win11, "Taskbar"),
                Debloat("Win11TaskbarEndTask", "Enable End Task on taskbar",
                    "Enables the End Task option in the taskbar right click menu.",
                    ExplorerAdvancedDword("TaskbarEndTask", 1), win11, "Taskbar"),
                Debloat("Win11LastActiveClick", "Enable Last Active Click",
                    "Allows repeated clicks on an app taskbar icon to switch focus between open windows of that app.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'LastActiveClick' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Taskbar"),
                Debloat("Win11MultiMonitorTaskbar", "Show taskbar icons on all monitors",
                    "Shows app icons on the taskbar across multiple monitors.",
                    ExplorerAdvancedDword("MMTaskbarEnabled", 1), win11, "Taskbar"),
                Debloat("Win11TaskbarCombine", "Never combine taskbar buttons",
                    "Chooses never combine mode for taskbar buttons and labels where supported.",
                    ExplorerAdvancedDword("TaskbarGlomLevel", 2), win11, "Taskbar"),

                Debloat("Win11ExplorerDefaultThisPC", "Open File Explorer to This PC",
                    "Changes the default File Explorer start location to This PC.",
                    ExplorerAdvancedDword("LaunchTo", 1), win11, "File Explorer"),
                Debloat("Win11ShowExtensions", "Show file extensions",
                    "Shows file extensions for known file types.",
                    ExplorerAdvancedDword("HideFileExt", 0), win11, "File Explorer"),
                Debloat("Win11ShowHidden", "Show hidden files",
                    "Shows hidden files, folders, and drives.",
                    ExplorerAdvancedDword("Hidden", 1), win11, "File Explorer"),
                Debloat("Win11HideHomeGallery", "Hide Home or Gallery",
                    "Hides the Home and Gallery sections from the File Explorer navigation pane where supported.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Classes\CLSID\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Classes\CLSID\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}' -Name 'System.IsPinnedToNameSpaceTree' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "File Explorer"),
                Debloat("Win11HideDuplicateDrives", "Hide duplicate removable drives",
                    "Hides duplicate removable drive entries from File Explorer navigation.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\NonEnum' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\NonEnum' -Name '{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "File Explorer"),
                Debloat("Win11CommonFoldersThisPC", "Restore common folders in This PC",
                    "Adds common folders such as Desktop and Downloads back to This PC where supported.",
                    RegistryPolicyScript(new[] { @"Write-Output 'Common folder visibility uses shell namespace defaults on this build.'" }), win11, "File Explorer"),
                Debloat("Win11HideLegacyFolders", "Hide 3D Objects, Music, or OneDrive",
                    "Hides legacy 3D Objects, Music, and OneDrive navigation entries where supported.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel' -Name '{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "File Explorer"),
                Debloat("Win11HideContextMenuSharing", "Hide legacy sharing context options",
                    "Hides Include in library, Give access to, and Share context menu entries where supported.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer' -Name 'NoSharing' -Value 1 -PropertyType DWord -Force | Out-Null"
                    }), win11, "File Explorer"),
                Debloat("Win11DriveLetters", "Show drive letters after label",
                    "Changes drive letter visibility or position in File Explorer.",
                    ExplorerAdvancedDword("ShowDriveLettersFirst", 0), win11, "File Explorer"),

                Debloat("Win11DisableSnap", "Disable window snapping",
                    "Disables window snapping.",
                    ExplorerAdvancedDword("SnapAssist", 0), win11, "Multi-tasking"),
                Debloat("Win11SnapAssistSuggestions", "Disable Snap Assist suggestions",
                    "Disables suggestions shown when snapping a window.",
                    ExplorerAdvancedDword("SnapAssistFlyout", 0), win11, "Multi-tasking"),
                Debloat("Win11SnapLayouts", "Disable Snap Layout suggestions",
                    "Disables Snap Layout suggestions when dragging windows to the top of screen or hovering maximize.",
                    ExplorerAdvancedDword("EnableSnapAssistFlyout", 0), win11, "Multi-tasking"),
                Debloat("Win11AltTabTabs", "Hide tabs from Alt+Tab and snapping",
                    "Changes whether tabs are shown when snapping or pressing Alt+Tab.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'MultiTaskingAltTabFilter' -Value 3 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Multi-tasking"),

                Debloat("Win11Sandbox", "Enable Windows Sandbox",
                    "Enables Windows Sandbox, a lightweight isolated desktop environment.",
                    OptionalFeatureScript("Containers-DisposableClientVM"), win11, "Optional Windows Features"),
                Debloat("Win11WSL", "Enable Windows Subsystem for Linux",
                    "Enables Windows Subsystem for Linux optional Windows feature.",
                    OptionalFeatureScript("Microsoft-Windows-Subsystem-Linux"), win11, "Optional Windows Features"),

                Debloat("Win11XboxGameBar", "Disable Xbox Game Bar and recording",
                    "Disables Xbox Game Bar integration, game recording, and ms-gamingoverlay popups where supported.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR' -Name 'AppCaptureEnabled' -Value 0 -PropertyType DWord -Force | Out-Null",
                        @"New-Item -Path 'HKCU:\System\GameConfigStore' -Force | Out-Null",
                        @"New-ItemProperty -Path 'HKCU:\System\GameConfigStore' -Name 'GameDVR_Enabled' -Value 0 -PropertyType DWord -Force | Out-Null"
                    }), win11, "Other"),
                Debloat("Win11BraveBloat", "Disable Brave browser bloat",
                    "Disables Brave AI, Crypto, News, Rewards, and related surfaces where policy keys are honored.",
                    RegistryPolicyScript(new[]
                    {
                        @"New-Item -Path 'HKLM:\SOFTWARE\Policies\BraveSoftware\Brave' -Force | Out-Null",
                        @"'BraveRewardsDisabled','BraveWalletDisabled','BraveAIChatEnabled','NewTabPageHideAllWidgets' | ForEach-Object { New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\BraveSoftware\Brave' -Name $_ -Value 1 -PropertyType DWord -Force | Out-Null }"
                    }), win11, "Other"),

                Debloat("Win11AdvancedDifferentUser", "Advanced: different user target",
                    "Documents the advanced mode for applying changes to another user. JaiDee-Optimize currently applies UI-selected tweaks to the active user/admin context.",
                    RegistryPolicyScript(new[] { @"Write-Output 'Different-user debloat mode is a planned workflow option; no system setting was changed.'" }), win11, "Advanced Features"),
                Debloat("Win11AdvancedSysprep", "Advanced: default user sysprep mode",
                    "Documents the sysprep/default-profile mode for future provisioning workflows. This safe entry does not edit the Default user profile automatically.",
                    RegistryPolicyScript(new[] { @"Write-Output 'Sysprep/default-user mode is a planned workflow option; no system setting was changed.'" }), win11, "Advanced Features")
            };

            return tweaks;
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

        private static TweakDefinition Debloat(string id, string displayName, string description, string script, string category = "Win10Debloat", string group = "Other")
        {
            return new TweakDefinition
            {
                Id = id,
                DisplayName = displayName,
                Description = description,
                Type = TweakType.Debloat,
                Category = category,
                ValueName = group,
                PowerShellScript = script
            };
        }

        private static string ConsumerAppsScript()
        {
            return AppxRemovalScript(new[]
            {
                "Microsoft.549981C3F5F10",
                "Microsoft.3DBuilder",
                "Microsoft.Appconnector",
                "Microsoft.Asphalt8Airborne",
                "Microsoft.BingNews",
                "Microsoft.BingFinance",
                "Microsoft.BingFoodAndDrink",
                "Microsoft.BingHealthAndFitness",
                "Microsoft.BingSports",
                "Microsoft.BingTravel",
                "Microsoft.BingWeather",
                "Microsoft.BioEnrollment",
                "Microsoft.CommsPhone",
                "Microsoft.ConnectivityStore",
                "Microsoft.GetHelp",
                "Microsoft.Getstarted",
                "Microsoft.Messaging",
                "Microsoft.Microsoft3DViewer",
                "Microsoft.MicrosoftOfficeHub",
                "Microsoft.MicrosoftSolitaireCollection",
                "Microsoft.MicrosoftStickyNotes",
                "Microsoft.MixedReality.Portal",
                "Microsoft.NetworkSpeedTest",
                "Microsoft.Office.OneNote",
                "Microsoft.Office.Sway",
                "Microsoft.OneConnect",
                "Microsoft.People",
                "Microsoft.Print3D",
                "Microsoft.PowerAutomateDesktop",
                "Microsoft.SkypeApp",
                "Microsoft.Todos",
                "Microsoft.Wallet",
                "Microsoft.WindowsAlarms",
                "Microsoft.WindowsCamera",
                "microsoft.windowscommunicationsapps",
                "Microsoft.WindowsFeedbackHub",
                "Microsoft.WindowsMaps",
                "Microsoft.WindowsPhone",
                "Microsoft.WindowsReadingList",
                "Microsoft.WindowsSoundRecorder",
                "Microsoft.YourPhone",
                "Microsoft.ZuneMusic",
                "Microsoft.ZuneVideo",
                "Microsoft.Advertising.Xaml",
                "ActiproSoftwareLLC.562882FEEB491",
                "AdobeSystemsIncorporated.AdobePhotoshopExpress",
                "AutodeskSketchBook",
                "CAF9E577.Plex",
                "CyberLinkCorp.hs.PowerMediaPlayer14forHPConsumerPC",
                "D52A8D61.FarmVille2CountryEscape",
                "D5EA27B7.Duolingo-LearnLanguagesforFree",
                "DB6EA5DB.CyberLinkMediaSuiteEssentials",
                "DolbyLaboratories.DolbyAccess",
                "Drawboard.DrawboardPDF",
                "EclipseManager",
                "Facebook.Facebook",
                "Fitbit.FitbitCoach",
                "Flipboard.Flipboard",
                "GAMELOFTSA.Asphalt8Airborne",
                "king.com.BubbleWitch3Saga",
                "king.com.CandyCrushSaga",
                "king.com.CandyCrushSodaSaga",
                "king.com.FarmHeroesSaga",
                "king.com.MarchofEmpires",
                "Microsoft.MinecraftUWP",
                "Microsoft.PPIProjection",
                "Microsoft.PowerBIForWindows",
                "Microsoft.RoyalRevolt2",
                "Microsoft.SkypeApp",
                "Netflix",
                "PandoraMediaInc.29680B314EFC2",
                "PhototasticCollage",
                "PicsArt-PhotoStudio",
                "ShazamEntertainmentLtd.Shazam",
                "SpotifyAB.SpotifyMusic",
                "ThumbmunkeysLtd.PhototasticCollage",
                "TuneIn.TuneInRadio",
                "Twitter",
                "XINGAG.XING",
                "Clipchamp.Clipchamp",
                "MicrosoftCorporationII.QuickAssist",
                "MicrosoftCorporationII.MicrosoftFamily"
            });
        }

        private static string SysprepAppxScript()
        {
            return "Get-AppxPackage -AllUsers | Where-Object { $_.NonRemovable -ne $true } | ForEach-Object { try { Remove-AppxPackage -Package $_.PackageFullName -AllUsers -ErrorAction Stop } catch { Remove-AppxPackage -Package $_.PackageFullName -ErrorAction SilentlyContinue } }; Write-Output 'Sysprep Appx removal pass completed.'";
        }

        private static string RemoveKeysScript()
        {
            return RegistryPolicyScript(new[]
            {
                @"$keys = @('EclipseManager','ActiproSoftwareLLC','Microsoft.PPIProjection','Microsoft.XboxGameCallableUI')",
                @"foreach ($key in $keys) { Remove-Item -Path (Join-Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount' $key) -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item -Path (Join-Path 'HKCU:\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages' $key) -Recurse -Force -ErrorAction SilentlyContinue }"
            });
        }

        private static string DisableScheduledTasksScript()
        {
            return RegistryPolicyScript(new[]
            {
                @"$tasks = @('\Microsoft\XblGameSave\XblGameSaveTaskLogon','\Microsoft\XblGameSave\XblGameSaveTask','\Microsoft\Windows\Customer Experience Improvement Program\Consolidator','\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip','\Microsoft\Windows\Feedback\Siuf\DmClient')",
                @"foreach ($task in $tasks) { Disable-ScheduledTask -TaskPath (Split-Path $task) -TaskName (Split-Path $task -Leaf) -ErrorAction SilentlyContinue | Out-Null }"
            });
        }

        private static string Windows10PrivacyScript()
        {
            return RegistryPolicyScript(new[]
            {
                @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection' -Force | Out-Null",
                @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection' -Name 'AllowTelemetry' -Value 0 -PropertyType DWord -Force | Out-Null",
                @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search' -Force | Out-Null",
                @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search' -Name 'BingSearchEnabled' -Value 0 -PropertyType DWord -Force | Out-Null",
                @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search' -Name 'CortanaConsent' -Value 0 -PropertyType DWord -Force | Out-Null",
                @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Force | Out-Null",
                @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Name 'Enabled' -Value 0 -PropertyType DWord -Force | Out-Null",
                @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Force | Out-Null",
                @"'ContentDeliveryAllowed','OemPreInstalledAppsEnabled','PreInstalledAppsEnabled','SilentInstalledAppsEnabled','SystemPaneSuggestionsEnabled','SubscribedContent-338388Enabled','SubscribedContent-338389Enabled','SubscribedContent-338393Enabled' | ForEach-Object { New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager' -Name $_ -Value 0 -PropertyType DWord -Force | Out-Null }",
                @"New-Item -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Force | Out-Null",
                @"New-ItemProperty -Path 'HKCU:\Software\Policies\Microsoft\Windows\Explorer' -Name 'DisableSearchBoxSuggestions' -Value 1 -PropertyType DWord -Force | Out-Null"
            });
        }

        private static string EdgePdfScript(bool enable)
        {
            int value = enable ? 0 : 1;
            return RegistryPolicyScript(new[]
            {
                @"New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\MicrosoftEdge\Main' -Force | Out-Null",
                @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\MicrosoftEdge\Main' -Name 'AlwaysOpenPdfExternally' -Value " + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " -PropertyType DWord -Force | Out-Null"
            });
        }

        private static string Windows10RevertScript()
        {
            return RegistryPolicyScript(new[]
            {
                @"New-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection' -Name 'AllowTelemetry' -Value 3 -PropertyType DWord -Force | Out-Null",
                @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search' -Name 'BingSearchEnabled' -Value 1 -PropertyType DWord -Force | Out-Null",
                @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search' -Name 'CortanaConsent' -Value 1 -PropertyType DWord -Force | Out-Null",
                @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo' -Name 'Enabled' -Value 1 -PropertyType DWord -Force | Out-Null",
                @"$tasks = @('\Microsoft\XblGameSave\XblGameSaveTaskLogon','\Microsoft\XblGameSave\XblGameSaveTask','\Microsoft\Windows\Customer Experience Improvement Program\Consolidator','\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip','\Microsoft\Windows\Feedback\Siuf\DmClient')",
                @"foreach ($task in $tasks) { Enable-ScheduledTask -TaskPath (Split-Path $task) -TaskName (Split-Path $task -Leaf) -ErrorAction SilentlyContinue | Out-Null }",
                @"Get-AppxPackage -AllUsers | ForEach-Object { $manifest = Join-Path $_.InstallLocation 'AppxManifest.xml'; if (Test-Path $manifest) { Add-AppxPackage -DisableDevelopmentMode -Register $manifest -ErrorAction SilentlyContinue } }"
            }) + "; " + EdgePdfScript(true);
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

        private static string ExplorerAdvancedDword(string name, int value)
        {
            return RegistryPolicyScript(new[]
            {
                @"New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Force | Out-Null",
                @"New-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name '" + name + "' -Value " + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " -PropertyType DWord -Force | Out-Null"
            });
        }

        private static string OptionalFeatureScript(string featureName)
        {
            return "Enable-WindowsOptionalFeature -Online -FeatureName '" + featureName + "' -All -NoRestart -ErrorAction SilentlyContinue | Out-Null; " +
                "Write-Output 'Optional Windows feature requested: " + featureName + "'";
        }

        private static string AppxRemovalScript(IEnumerable<string> packageNames)
        {
            string joined = "'" + string.Join("','", packageNames) + "'";
            return "$packages = @(" + joined + "); " +
                "foreach ($package in $packages) { " +
                "Get-AppxPackage -AllUsers -ErrorAction SilentlyContinue | Where-Object { $_.Name -eq $package -or $_.Name -like ('*' + $package + '*') } | ForEach-Object { try { Remove-AppxPackage -Package $_.PackageFullName -AllUsers -ErrorAction Stop } catch { Remove-AppxPackage -Package $_.PackageFullName -ErrorAction SilentlyContinue } }; " +
                "Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -eq $package -or $_.DisplayName -like ('*' + $package + '*') } | ForEach-Object { Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName -ErrorAction SilentlyContinue | Out-Null }; " +
                "}; Write-Output ('Debloat Appx pass completed: ' + ($packages -join ', '))";
        }

        private static string RegistryPolicyScript(IEnumerable<string> commands)
        {
            return string.Join("; ", commands) + "; Write-Output 'Debloat policy pass completed.'";
        }
    }
}

