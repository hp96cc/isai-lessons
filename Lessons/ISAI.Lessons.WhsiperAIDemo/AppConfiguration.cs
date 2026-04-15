using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace ISAI.Lessons.WhsiperAIDemo
{
    /// <summary>
    /// Centralized application configuration loaded from appsettings.json
    /// with platform-specific overrides for cross-platform compatibility.
    /// </summary>
    public sealed class AppConfiguration
    {
        private static readonly Lazy<AppConfiguration> _instance = new(() => LoadConfiguration());

        public static AppConfiguration Instance => _instance.Value;

        public ApplicationSettings Application { get; init; }
        public OneDriveSettings OneDrive { get; init; }
        public GraphSettings Graph { get; init; }
        public RagDatabaseSettings RagDatabase { get; init; }
        public TranslationSettings Translation { get; init; }

        private AppConfiguration()
        {
            Application = new ApplicationSettings();
            OneDrive = new OneDriveSettings();
            Graph = new GraphSettings();
            RagDatabase = new RagDatabaseSettings();
            Translation = new TranslationSettings();
        }

        private static AppConfiguration LoadConfiguration()
        {
            try
            {
                // Determine platform-specific config file
                string baseConfigPath = "appsettings.json";
                string platformConfigPath = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? "appsettings.macos.json"
                    : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                        ? "appsettings.linux.json"
                        : baseConfigPath;

                // Load base configuration
                if (!File.Exists(baseConfigPath))
                {
                    Console.WriteLine($"Warning: {baseConfigPath} not found. Using default values.");
                    return new AppConfiguration();
                }

                var baseConfig = LoadConfigFile(baseConfigPath);

                // Merge with platform-specific overrides if available
                if (platformConfigPath != baseConfigPath && File.Exists(platformConfigPath))
                {
                    Console.WriteLine($"Loading platform-specific configuration: {platformConfigPath}");
                    var platformConfig = LoadConfigFile(platformConfigPath);
                    MergeConfigurations(baseConfig, platformConfig);
                }

                // Allow environment variable overrides
                ApplyEnvironmentVariableOverrides(baseConfig);

                Console.WriteLine($"Configuration loaded successfully for platform: {GetPlatformName()}");
                return baseConfig;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                Console.WriteLine("Falling back to default configuration.");
                return new AppConfiguration();
            }
        }

        private static AppConfiguration LoadConfigFile(string configPath)
        {
            string json = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            return JsonSerializer.Deserialize<AppConfiguration>(json, options) ?? new AppConfiguration();
        }

        private static void MergeConfigurations(AppConfiguration baseConfig, AppConfiguration platformConfig)
        {
            // Merge Application settings
            if (platformConfig.Application != null)
            {
                baseConfig.Application.BaseFolder = platformConfig.Application.BaseFolder ?? baseConfig.Application.BaseFolder;
                baseConfig.Application.FfmpegPath = platformConfig.Application.FfmpegPath ?? baseConfig.Application.FfmpegPath;
                baseConfig.Application.DefaultModelPath = platformConfig.Application.DefaultModelPath ?? baseConfig.Application.DefaultModelPath;
                baseConfig.Application.TesseractDataPath = platformConfig.Application.TesseractDataPath ?? baseConfig.Application.TesseractDataPath;
            }

            // Merge RagDatabase settings
            if (platformConfig.RagDatabase != null)
            {
                baseConfig.RagDatabase.OutputDirectory = platformConfig.RagDatabase.OutputDirectory ?? baseConfig.RagDatabase.OutputDirectory;
                baseConfig.RagDatabase.FramesDirectory = platformConfig.RagDatabase.FramesDirectory ?? baseConfig.RagDatabase.FramesDirectory;
            }

            // Merge Translation settings
            if (platformConfig.Translation != null)
            {
                baseConfig.Translation.OllamaBaseUrl = platformConfig.Translation.OllamaBaseUrl ?? baseConfig.Translation.OllamaBaseUrl;
                baseConfig.Translation.OllamaModel = platformConfig.Translation.OllamaModel ?? baseConfig.Translation.OllamaModel;
                if (platformConfig.Translation.TargetLanguages != null)
                    baseConfig.Translation.TargetLanguages = platformConfig.Translation.TargetLanguages;
                baseConfig.Translation.Enabled = platformConfig.Translation.Enabled;
                if (platformConfig.Translation.MaxCharsPerGroup > 0)
                    baseConfig.Translation.MaxCharsPerGroup = platformConfig.Translation.MaxCharsPerGroup;
            }
        }

        private static void ApplyEnvironmentVariableOverrides(AppConfiguration config)
        {
            // Allow environment variables to override configuration
            config.Application.BaseFolder = Environment.GetEnvironmentVariable("APP_BASE_FOLDER") ?? config.Application.BaseFolder;
            config.Application.FfmpegPath = Environment.GetEnvironmentVariable("APP_FFMPEG_PATH") ?? config.Application.FfmpegPath;
            
            config.Graph.ClientId = Environment.GetEnvironmentVariable("GRAPH_CLIENT_ID") ?? config.Graph.ClientId;
            config.Graph.TenantId = Environment.GetEnvironmentVariable("GRAPH_TENANT_ID") ?? config.Graph.TenantId;
            config.Graph.ClientSecret = Environment.GetEnvironmentVariable("GRAPH_CLIENT_SECRET") ?? config.Graph.ClientSecret;
            config.Graph.TargetUser = Environment.GetEnvironmentVariable("GRAPH_TARGET_USER") ?? config.Graph.TargetUser;
        }

        private static string GetPlatformName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "macOS";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";
            return "Unknown";
        }

        public sealed class ApplicationSettings
        {
            public string BaseFolder { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Temp\SOL Video Processing\"
                : "/tmp/SOL Video Processing/";

            public string FfmpegPath { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\ffmpeg\bin\ffmpeg.exe"
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? "/opt/homebrew/bin/ffmpeg"
                    : "ffmpeg";

            public string DefaultModelPath { get; set; } = "ggml-medium.bin";
            public string TesseractDataPath { get; set; } = "./tessdata";
        }

        public sealed class OneDriveSettings
        {
            public string VideoUploadsFolder { get; set; } = "VideoUploads";
            public string ProcessedSubFolder { get; set; } = "processed";
            public string AutomatedUploadsFolder { get; set; } = "AutomatedUploads";

            public string AutomatedProcessedUploadsFolder { get; set; } = "AutomatedUploads/Processed";

            public string SubtitlesUploadFolder { get; set; } = "SubtitlesUploadFolder";

        }

        public sealed class GraphSettings
        {
            public string ClientId { get; set; } = "";
            public string TenantId { get; set; } = "";
            public string ClientSecret { get; set; } = "";
            public string TargetUser { get; set; } = "";
        }

        public sealed class RagDatabaseSettings
        {
            public string CollectionName { get; set; } = "6b0ac5fd-fc60-4cff-9fb1-bcee64832128";
            public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
            public string OllamaModel { get; set; } = "moondream";
            
            public string OutputDirectory { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Temp\SOL RAG Test\segments"
                : "/tmp/SOL RAG Test/segments";
                
            public string FramesDirectory { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Temp\SOL RAG Test\frames"
                : "/tmp/SOL RAG Test/frames";
        }

        public sealed class TranslationSettings
        {
            public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
            public string OllamaModel { get; set; } = "gemma3:12b";
            public List<string> TargetLanguages { get; set; } = new List<string> { "es", "fr", "de" };
            public bool Enabled { get; set; } = true;
            public int MaxCharsPerGroup { get; set; } = 500;
        }
    }
}