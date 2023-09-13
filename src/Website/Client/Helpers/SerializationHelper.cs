using System.Text;
using System.Xml;
using System.Xml.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Website.Client.Helpers
{
    public class SerializationHelper
    {
        public static string SerializeObjectToXML<T>(T obj)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var s1 = new XmlSerializer(typeof(T));
            var builder = new StringBuilder();
            var xmlws = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
            using (var writer = XmlWriter.Create(builder, xmlws))
            {
                s1.Serialize(writer, obj, ns);
            }

            return builder.ToString();
        }

        public static string SerializeObjectToYaml<T>(T obj)
        {
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            return serializer.Serialize(obj);
        }
    }
}
