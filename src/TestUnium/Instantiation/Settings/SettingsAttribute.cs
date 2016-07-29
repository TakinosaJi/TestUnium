﻿using System;
using System.IO;
using Newtonsoft.Json;
using TestUnium.Common;
using TestUnium.Instantiation.Customization;
using TestUnium.Instantiation.Customization.Prioritizing;

namespace TestUnium.Instantiation.Settings
{
    [TheOnly]
    [Priority((UInt16)CustomizationAttributePriorities.Settings)]
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsAttribute : CustomizationAttribute, ICustomizer<SettingsDrivenTest>
    {
        private readonly Type _settingsType;
        private readonly Boolean _loadFromFile;
        private readonly Boolean _createFileIfNotExist;

        public SettingsAttribute(Type settingsType, Boolean loadFromFile = true, Boolean createFileIfNotExist = true) 
            : base(new []
            {
                typeof(NoSettingsAttribute)
            })
        {
            if (!typeof(ISettings).IsAssignableFrom(settingsType))
                throw new IncorrectInheritanceException(new[] { settingsType.Name }, new [] { nameof(SettingsBase)});
            _settingsType = settingsType;
            _loadFromFile = loadFromFile;
            _createFileIfNotExist = createFileIfNotExist;
        }  

        public virtual void Customize(SettingsDrivenTest context)
        {
            context.Settings = (ISettings)Activator.CreateInstance(_settingsType);

            var settingsFilePath = ShellHelper.TryGetArg(CommandLineArgsConstants.SettingsCmdArg, "settings.json");

            if (File.Exists(settingsFilePath))
            {
                if (_loadFromFile)
                {
                    context.Settings =
                        (ISettings) JsonConvert.DeserializeObject(File.ReadAllText(settingsFilePath), _settingsType);
                }
            }
            else
            {
                if (_createFileIfNotExist)
                {
                    context.Settings = (ISettings) Activator.CreateInstance(context.Settings.GetType());
                    File.WriteAllText(settingsFilePath,
                        JsonConvert.SerializeObject(context.Settings, Formatting.Indented));
                }
            }

            context.Settings.PostDeserializationAction();
        }
    }
}