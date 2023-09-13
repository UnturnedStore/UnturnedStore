using System;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace Website.Client.Models
{
    [XmlRoot("UnturnedStorePlugin")]
    public class LoaderConfigurationExample
    {
        [XmlAttribute]
        public string Name { get; set; }

        [YamlMember(Alias = "License")]
        public Guid License { get; set; }
        [YamlMember(Alias = "Branch")]
        public string Branch { get; set; }
        [YamlMember(Alias = "Version")]
        public string Version { get; set; }
        [YamlMember(Alias = "Enabled")]
        public bool Enabled { get; set; }
    }
}
