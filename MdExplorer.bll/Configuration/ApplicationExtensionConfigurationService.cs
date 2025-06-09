using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using MdExplorer.Features.Configuration.Interfaces;
using MdExplorer.Features.Configuration.Models;

namespace MdExplorer.Features.Configuration.Services
{
    public class ApplicationExtensionConfigurationService : IApplicationExtensionConfiguration
    {
        private readonly ILogger<ApplicationExtensionConfigurationService> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private ApplicationExtensionConfigurationModel _configuration;
        private readonly string _configFilePath;

        public ApplicationExtensionConfigurationService(
            ILogger<ApplicationExtensionConfigurationService> logger,
            FileSystemWatcher fileSystemWatcher)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            _configFilePath = Path.Combine(_fileSystemWatcher.Path, ".mdapplicationtoopen");
            LoadConfiguration();
        }

        public List<string> GetSupportedExtensions()
        {
            return _configuration.SupportedExtensions ?? new List<string>();
        }

        public bool IsExtensionSupported(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;

            // Remove dot if present
            extension = extension.TrimStart('.');
            
            return _configuration.SupportedExtensions != null && 
                   _configuration.SupportedExtensions.Any(ext => 
                       string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase));
        }

        public void ReloadConfiguration()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            if (File.Exists(_configFilePath))
            {
                try
                {
                    var yamlContent = File.ReadAllText(_configFilePath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    
                    _configuration = deserializer.Deserialize<ApplicationExtensionConfigurationModel>(yamlContent);
                    _logger.LogInformation($"Loaded application extension configuration from {_configFilePath} with {_configuration.SupportedExtensions?.Count ?? 0} extensions");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load .mdapplicationtoopen configuration. Using default values.");
                    LoadDefaultConfiguration();
                }
            }
            else
            {
                _logger.LogWarning($".mdapplicationtoopen file not found at {_configFilePath}. Using default values.");
                LoadDefaultConfiguration();
            }
        }

        private void LoadDefaultConfiguration()
        {
            _configuration = new ApplicationExtensionConfigurationModel
            {
                SupportedExtensions = new List<string> { "xlsx", "pdf", "bmpr", "docx", "pptx", "xls", "ppt" }
            };
        }
    }
}