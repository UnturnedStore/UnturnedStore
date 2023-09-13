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
            XmlSerializerNamespaces ns = new();
            ns.Add("", "");
            XmlSerializer s1 = new(typeof(T));
            StringBuilder builder = new();
            XmlWriterSettings xmlws = new() { OmitXmlDeclaration = true, Indent = true };
            using (XmlWriter writer = XmlWriter.Create(builder, xmlws))
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
