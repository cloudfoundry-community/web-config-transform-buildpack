using System;

namespace Web.Config.Transform.Buildpack
{
    public class ConfigReaderWriterBuilder
    {
        public ConfigReaderWriterBuilder(Action<IConfigManagerSettingsBuilder> settingBuilder)
            : this(new ConfigManagerSettingsBuilder(), settingBuilder) { }

        public ConfigReaderWriterBuilder(IConfigManagerSettingsBuilder settingsBuilder, Action<IConfigManagerSettingsBuilder> settingBuilder)
        {
            ConfigManagerSettingsBuilder = settingsBuilder;
            SettingBuilderAction = settingBuilder;
        }

        public IConfigManagerSettingsBuilder ConfigManagerSettingsBuilder { get; }
        public Action<IConfigManagerSettingsBuilder> SettingBuilderAction { get; }
        
        public ConfigReaderWriter Build()
        {
            SettingBuilderAction(ConfigManagerSettingsBuilder);
            return new ConfigReaderWriter(ConfigManagerSettingsBuilder.Build());
        }
    }
}
