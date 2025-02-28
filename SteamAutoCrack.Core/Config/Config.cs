﻿using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SteamAutoCrack.Core.Utils;
using static SteamAutoCrack.Core.Utils.SteamStubUnpackerConfig;

namespace SteamAutoCrack.Core.Config;

public class Config
{
    /// <summary>
    ///     Program Language.
    /// </summary>
    public enum Languages
    {
        [Description("English")] en_US,
        [Description("中文")] zh_CN
    }

    private static readonly ILogger _log = Log.ForContext<Config>();

    /// <summary>
    ///     Temp file path.
    /// </summary>
    public static string TempPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TEMP");

    /// <summary>
    ///     Config file path.
    /// </summary>
    public static string ConfigPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    /// <summary>
    ///     Steam emulator config path.
    /// </summary>
    public static string EMUConfigPath { get; set; } = Path.Combine(TempPath, "steam_settings");

    /// <summary>
    ///     Path of steam emulator files.
    /// </summary>
    public static string GoldbergPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Goldberg");

    /// <summary>
    ///     Path to process.
    /// </summary>
    public static string InputPath { get; set; } = string.Empty;

    /// <summary>
    ///     Goldberg emulator job ID.
    /// </summary>
    public static string GoldbergVersion { get; set; } = GetGoldbergVersion();

    /// <summary>
    ///     Enable Crack Applier Mode.
    /// </summary>
    public static bool CrackApplierMode { get; set; } = CheckCrackApplierMode();

    /// <summary>
    ///     Save Crack Process Config.
    /// </summary>
    public static bool SaveCrackConfig
    {
        get => _SaveCrackConfig;
        set
        {
            if (value)
                SaveConfig();
            else
                File.Delete(ConfigPath);
            _SaveCrackConfig = value;
        }
    }

    /// <summary>
    ///     Enable debug log.
    /// </summary>
    public static bool EnableDebugLog
    {
        get => _EnableDebugLog;
        set
        {
            _EnableDebugLog = value;
            if (value)
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
            else
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
        }
    }

    /// <summary>
    ///    Program language.
    /// </summary>
    public static Languages Language
    {
        get => _language;
        set
        {
            if (_language != value)
            {
                _language = value;
                OnLanguageChanged?.Invoke(_language);
            }
        }
    }

    /// <summary>
    ///     Output log to file.
    /// </summary>
    public static bool LogToFile { get; set; }

    public delegate void LanguageChangedHandler(Languages newLanguage);
    public static event LanguageChangedHandler? OnLanguageChanged;

    private static Languages _language { get; set; } = GetDefaultLanguage();

    public static EMUApplyConfigs EMUApplyConfigs { get; set; } = new();
    public static EMUConfigs EMUConfigs { get; set; } = new();
    public static SteamStubUnpackerConfigs SteamStubUnpackerConfigs { get; set; } = new();
    public static EMUGameInfoConfigs EMUGameInfoConfigs { get; set; } = new();
    public static GenCrackOnlyConfigs GenCrackOnlyConfigs { get; set; } = new();
    public static ProcessConfigs ProcessConfigs { get; set; } = new();

    private static bool _SaveCrackConfig = CheckConfigFile();
    private static bool _EnableDebugLog;
    public static LoggingLevelSwitch loggingLevelSwitch = new();

    private static bool CheckConfigFile()
    {
        if (File.Exists(ConfigPath)) return true;
        return false;
    }

    public static void CheckInputPath()
    {
        if (!Directory.Exists(EMUConfigPath) && !File.Exists(EMUConfigPath)) throw new Exception("Invaild input path.");
    }

    public static string GetGoldbergVersion()
    {
        try
        {
            var ver = File.ReadLines(Path.Combine(GoldbergPath, "commit_id")).First();

            return ver;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get Goldberg Steam emulator version.");
            return "N/A";
        }
    }

    public static void ResettoDefaultAll()
    {
        EMUConfigPath = Path.Combine(TempPath, "steam_settings");
        GoldbergPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Goldberg");
        TempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TEMP");
        EnableDebugLog = false;
        LogToFile = false;
        Language = GetDefaultLanguage();
        ResettoDefaultConfigs();
    }

    public static void ResettoDefaultConfigs()
    {
        EMUApplyConfigs.ResettoDefault();
        EMUConfigs.ResettoDefault();
        SteamStubUnpackerConfigs.ResettoDefault();
        EMUGameInfoConfigs.ResettoDefault();
        GenCrackOnlyConfigs.ResettoDefault();
        ProcessConfigs.ResettoDefault();
    }

    public static bool CheckCrackApplierMode()
    {
        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Apply_Crack"))) return true;
        return false;
    }

    public static void SaveConfig()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(new Configs
            {
                EMUApplyConfigs = EMUApplyConfigs,
                EMUConfigs = EMUConfigs,
                SteamStubUnpackerConfigs = SteamStubUnpackerConfigs,
                EMUGameInfoConfigs = EMUGameInfoConfigs,
                GenCrackOnlyConfigs = GenCrackOnlyConfigs,
                ProcessConfigs = ProcessConfigs,
                EnableDebugLog = EnableDebugLog,
                LogToFile = LogToFile,
                Language = Language
            }, options);
            File.WriteAllText(ConfigPath, jsonString);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error in saving config file.");
        }
    }

    public static bool LoadConfig()
    {
        try
        {
            var jsonString = File.ReadAllText(ConfigPath);
            var configs = JsonSerializer.Deserialize<Configs>(jsonString);
            if (configs != null)
            {
                EMUApplyConfigs = configs.EMUApplyConfigs ?? EMUApplyConfigs;
                EMUConfigs = configs.EMUConfigs ?? EMUConfigs;
                SteamStubUnpackerConfigs = configs.SteamStubUnpackerConfigs ?? SteamStubUnpackerConfigs;
                EMUGameInfoConfigs = configs.EMUGameInfoConfigs ?? EMUGameInfoConfigs;
                GenCrackOnlyConfigs = configs.GenCrackOnlyConfigs ?? GenCrackOnlyConfigs;
                ProcessConfigs = configs.ProcessConfigs ?? ProcessConfigs;
                EnableDebugLog = configs.EnableDebugLog;
                LogToFile = configs.LogToFile;
                Language = configs.Language;
            }

            _log.Information("Config loaded.");
            return true;
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Error in reading config file. Restoring to default value...");
            ResettoDefaultConfigs();
            return false;
        }
    }

    public static Languages GetDefaultLanguage()
    {
        var culture = CultureInfo.InstalledUICulture.Name;
        switch (culture.Substring(0, 2))
        {
            case "zh":
                return Languages.zh_CN;
            default:
                return Languages.en_US;
        }
    }

    public static string GetLanguage()
    {
        switch (_language)
        {
            case Languages.en_US:
                return "en-US";
            case Languages.zh_CN:
                return "zh-CN";
            default:
                return "en-US";
        }
    }
}

public class Configs
{
    public EMUApplyConfigs? EMUApplyConfigs { get; set; }
    public EMUConfigs? EMUConfigs { get; set; }
    public SteamStubUnpackerConfigs? SteamStubUnpackerConfigs { get; set; }
    public EMUGameInfoConfigs? EMUGameInfoConfigs { get; set; }
    public GenCrackOnlyConfigs? GenCrackOnlyConfigs { get; set; }
    public ProcessConfigs? ProcessConfigs { get; set; }
    public bool EnableDebugLog { get; set; }
    public bool LogToFile { get; set; }
    public Config.Languages Language { get; set; }
}

public class EMUApplyConfigs
{
    /// <summary>
    ///     Emulator save location.
    /// </summary>
    public string LocalSave { get; set; } = EMUApplyConfigDefault.LocalSave;

    /// <summary>
    ///     Enable change default emulator save location.
    /// </summary>
    public bool UseLocalSave { get; set; } = EMUApplyConfigDefault.UseLocalSave;

    /// <summary>
    ///     Use Experimental version of goldberg emulator.
    /// </summary>
    public bool UseGoldbergExperimental { get; set; } = EMUApplyConfigDefault.UseGoldbergExperimental;

    /// <summary>
    ///     Detect file sign date and generate steam_interfaces.txt
    /// </summary>
    public bool GenerateInterfacesFile { get; set; } = EMUApplyConfigDefault.GenerateInterfacesFile;

    /// <summary>
    ///     Force generate file steam_interfaces.txt (Ignore file sign date)
    /// </summary>
    public bool ForceGenerateInterfacesFiles { get; set; } = EMUApplyConfigDefault.ForceGenerateInterfacesFiles;

    public void ResettoDefault()
    {
        LocalSave = EMUApplyConfigDefault.LocalSave;
        UseLocalSave = EMUApplyConfigDefault.UseLocalSave;
        UseGoldbergExperimental = EMUApplyConfigDefault.UseGoldbergExperimental;
        GenerateInterfacesFile = EMUApplyConfigDefault.GenerateInterfacesFile;
        ForceGenerateInterfacesFiles = EMUApplyConfigDefault.ForceGenerateInterfacesFiles;
    }

    public EMUApplyConfig GetEMUApplyConfig()
    {
        return new EMUApplyConfig
        {
            ApplyPath = Config.InputPath,
            ConfigPath = Config.EMUConfigPath,
            GoldbergPath = Config.GoldbergPath,
            LocalSave = LocalSave,
            UseLocalSave = UseLocalSave,
            UseGoldbergExperimental = UseGoldbergExperimental,
            GenerateInterfacesFile = GenerateInterfacesFile,
            ForceGenerateInterfacesFiles = ForceGenerateInterfacesFiles
        };
    }
}

public class EMUConfigs
{
    /// <summary>
    ///     Set game language.
    /// </summary>
    public EMUConfig.Languages Language { get; set; } = EMUConfigDefault.Language;

    /// <summary>
    ///     Set Steam ID.
    /// </summary>
    public string SteamID { get; set; } = EMUConfigDefault.SteamID.ConvertToUInt64().ToString();

    /// <summary>
    ///     Set Steam account name.
    /// </summary>
    public string AccountName { get; set; } = EMUConfigDefault.AccountName;

    /// <summary>
    ///     Set custom emulator listen port.
    /// </summary>
    public string ListenPort { get; set; } = EMUConfigDefault.ListenPort.ToString();

    /// <summary>
    ///     Set Custom broadcast IP.
    /// </summary>
    public string CustomIP { get; set; } = EMUConfigDefault.CustomIP;

    /// <summary>
    ///     Generate custom_broadcasts.txt
    /// </summary>
    public bool UseCustomIP { get; set; } = EMUConfigDefault.UseCustomIP;

    /// <summary>
    ///     Disable all the networking functionality of the Steam emulator.
    /// </summary>
    public bool DisableNetworking { get; set; } = EMUConfigDefault.DisableNetworking;

    /// <summary>
    ///     Emable Steam emulator offline mode.
    /// </summary>
    public bool Offline { get; set; } = EMUConfigDefault.Offline;

    /// <summary>
    ///     Enable Steam emulator overlay.
    /// </summary>
    public bool EnableOverlay { get; set; } = EMUConfigDefault.EnableOverlay;

    public void ResettoDefault()
    {
        Language = EMUConfigDefault.Language;
        SteamID = EMUConfigDefault.SteamID.ConvertToUInt64().ToString();
        AccountName = EMUConfigDefault.AccountName;
        ListenPort = EMUConfigDefault.ListenPort.ToString();
        CustomIP = EMUConfigDefault.CustomIP;
        UseCustomIP = EMUConfigDefault.UseCustomIP;
        DisableNetworking = EMUConfigDefault.DisableNetworking;
        Offline = EMUConfigDefault.Offline;
        EnableOverlay = EMUConfigDefault.EnableOverlay;
    }

    public EMUConfig GetEMUConfig()
    {
        var emuConfig = new EMUConfig
        {
            AccountName = AccountName,
            UseCustomIP = UseCustomIP,
            DisableNetworking = DisableNetworking,
            Offline = Offline,
            EnableOverlay = EnableOverlay,
            ConfigPath = Config.EMUConfigPath,
            Language = Language
        };
        emuConfig.SetSteamIDFromString(SteamID);
        emuConfig.SetListenPortFromString(ListenPort);
        emuConfig.SetCustomIPFromString(CustomIP);
        return emuConfig;
    }
}

public class SteamStubUnpackerConfigs
{
    /// <summary>
    ///     Keeps the .bind section in the unpacked file.
    /// </summary>
    public bool KeepBind { get; set; } = SteamStubUnpackerConfigDefault.KeepBind;

    /// <summary>
    ///     Keeps the DOS stub in the unpacked file.
    /// </summary>
    public bool KeepStub { get; set; } = SteamStubUnpackerConfigDefault.KeepStub;

    /// <summary>
    ///     Realigns the unpacked file sections.
    /// </summary>
    public bool Realign { get; set; } = SteamStubUnpackerConfigDefault.Realign;

    /// <summary>
    ///     Recalculates the unpacked file checksum.
    /// </summary>
    public bool ReCalcChecksum { get; set; } = SteamStubUnpackerConfigDefault.ReCalcChecksum;

    /// <summary>
    ///     Use Experimental Features.
    /// </summary>
    public bool UseExperimentalFeatures { get; set; } = SteamStubUnpackerConfigDefault.UseExperimentalFeatures;

    /// <summary>
    ///     SteamAPICheckBypass Mode
    /// </summary>
    public SteamAPICheckBypassModes SteamAPICheckBypassMode { get; set; } = SteamAPICheckBypassModes.Disabled;

    /// <summary>
    ///     DLL hijacking name for SteamAPICheckBypass
    /// </summary>
    public SteamAPICheckBypassDLLs SteamAPICheckBypassDLL { get; set; } = SteamAPICheckBypassDLLs.WINMM_DLL;

    /// <summary>
    ///     SteamAPI Check Bypass Nth Time Setting
    /// </summary>
    public Int64 SteamAPICheckBypassNthTime { get; set; } = 1;

    public void ResettoDefault()
    {
        KeepBind = SteamStubUnpackerConfigDefault.KeepBind;
        KeepStub = SteamStubUnpackerConfigDefault.KeepStub;
        Realign = SteamStubUnpackerConfigDefault.Realign;
        ReCalcChecksum = SteamStubUnpackerConfigDefault.ReCalcChecksum;
        UseExperimentalFeatures = SteamStubUnpackerConfigDefault.UseExperimentalFeatures;
        SteamAPICheckBypassMode = SteamStubUnpackerConfigDefault.SteamAPICheckBypassMode;
        SteamAPICheckBypassDLL = SteamStubUnpackerConfigDefault.SteamAPICheckBypassDLL;
        SteamAPICheckBypassNthTime = SteamStubUnpackerConfigDefault.SteamAPICheckBypassNthTime;
    }

    public SteamStubUnpackerConfig GetSteamStubUnpackerConfig()
    {
        return new SteamStubUnpackerConfig
        {
            KeepBind = KeepBind,
            KeepStub = KeepStub,
            Realign = Realign,
            ReCalcChecksum = ReCalcChecksum,
            UseExperimentalFeatures = UseExperimentalFeatures,
            SteamAPICheckBypassMode = SteamAPICheckBypassMode,
            SteamAPICheckBypassDLL = SteamAPICheckBypassDLL,
            SteamAPICheckBypassNthTime = SteamAPICheckBypassNthTime
        };
    }
}

public class EMUGameInfoConfigs
{
    public EMUGameInfoConfig.GeneratorGameInfoAPI GameInfoAPI { get; set; } = EMUGameInfoConfigDefault.GameInfoAPI;

    /// <summary>
    ///     Required when using Steam official Web API.
    /// </summary>
    public string SteamWebAPIKey { get; set; } = EMUGameInfoConfigDefault.SteamWebAPIKey;

    /// <summary>
    ///     Enable generate game achievement images.
    /// </summary>
    public bool GenerateImages { get; set; } = EMUGameInfoConfigDefault.GenerateImages;

    [JsonIgnore] public string AppID { get; set; } = string.Empty;

    /// <summary>
    ///     Use Xan105 API for generating game schema.
    /// </summary>
    public bool UseXan105API { get; set; } = EMUGameInfoConfigDefault.UseXan105API;

    /// <summary>
    ///     Use Steam Web App List when generating DLCs.
    /// </summary>
    public bool UseSteamWebAppList { get; set; } = EMUGameInfoConfigDefault.UseSteamWebAppList;

    public void ResettoDefault()
    {
        SteamWebAPIKey = EMUGameInfoConfigDefault.SteamWebAPIKey;
        GameInfoAPI = EMUGameInfoConfigDefault.GameInfoAPI;
        GenerateImages = EMUGameInfoConfigDefault.GenerateImages;
        UseXan105API = EMUGameInfoConfigDefault.UseXan105API;
        UseSteamWebAppList = EMUGameInfoConfigDefault.UseSteamWebAppList;
    }

    public EMUGameInfoConfig GetEMUGameInfoConfig()
    {
        var emuGameInfoConfig = new EMUGameInfoConfig
        {
            GameInfoAPI = GameInfoAPI,
            SteamWebAPIKey = SteamWebAPIKey,
            GenerateImages = GenerateImages,
            UseXan105API = UseXan105API,
            UseSteamWebAppList = UseSteamWebAppList,
            ConfigPath = Config.EMUConfigPath
        };
        emuGameInfoConfig.SetAppIDFromString(AppID);
        return emuGameInfoConfig;
    }
}

public class GenCrackOnlyConfigs
{
    /// <summary>
    ///     Crack only file output path.
    /// </summary>
    public string OutputPath { get; set; } = GenCrackOnlyConfigDefault.OutputPath;

    /// <summary>
    ///     Create crack only readme file.
    /// </summary>
    public bool CreateReadme { get; set; } = GenCrackOnlyConfigDefault.CreateReadme;

    /// <summary>
    ///     Pack Crack only file with .zip archive.
    /// </summary>
    public bool Pack { get; set; } = GenCrackOnlyConfigDefault.Pack;

    public void ResettoDefault()
    {
        OutputPath = GenCrackOnlyConfigDefault.OutputPath;
        CreateReadme = GenCrackOnlyConfigDefault.CreateReadme;
        Pack = GenCrackOnlyConfigDefault.Pack;
    }

    public GenCrackOnlyConfig GetGenCrackOnlyConfig()
    {
        return new GenCrackOnlyConfig
        {
            SourcePath = Config.InputPath,
            OutputPath = OutputPath,
            CreateReadme = CreateReadme,
            Pack = Pack
        };
    }
}

public class ProcessConfigs
{
    /// <summary>
    ///     Generate Steam emulator Game Info.
    /// </summary>
    public bool GenerateEMUGameInfo { get; set; } = true;

    /// <summary>
    ///     Generate Steam emulator config.
    /// </summary>
    public bool GenerateEMUConfig { get; set; } = true;

    /// <summary>
    ///     Unpack Steamstub.
    /// </summary>
    public bool Unpack { get; set; } = true;

    /// <summary>
    ///     Apply Steam emulator.
    /// </summary>
    public bool ApplyEMU { get; set; } = true;

    /// <summary>
    ///     Generate Crack Only Files.
    /// </summary>
    public bool GenerateCrackOnly { get; set; }

    /// <summary>
    ///     Restore Crack.
    /// </summary>
    public bool Restore { get; set; }

    public void ResettoDefault()
    {
        GenerateEMUGameInfo = true;
        GenerateEMUConfig = true;
        Unpack = true;
        ApplyEMU = true;
        GenerateCrackOnly = false;
        Restore = false;
    }
}