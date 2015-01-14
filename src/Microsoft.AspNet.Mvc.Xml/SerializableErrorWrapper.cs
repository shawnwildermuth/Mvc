using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.AspNet.Mvc.Xml
{
    /// <summary>
    /// Wrapper class for <see cref="SerializableError"/> to enable it to
    /// be serialized by the xml formatters.
    /// </summary>
    [XmlRoot("Error")]
    public sealed class SerializableErrorWrapper : IXmlSerializable
    {
        // Note: XmlSerializer requires to have default constructor
        public SerializableErrorWrapper()
        {
            SerializableError = new SerializableError();
        }

        public SerializableErrorWrapper([NotNull] SerializableError error)
        {
            SerializableError = error;
        }

        public SerializableError SerializableError { get; }

        /// <inheritdoc />
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <inheritdoc />
        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }

            reader.ReadStartElement();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var key = XmlConvert.DecodeName(reader.LocalName);
                var value = reader.ReadInnerXml();

                SerializableError.Add(key, value);
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        /// <inheritdoc />
        public void WriteXml(XmlWriter writer)
        {
            foreach (var keyValuePair in SerializableError)
            {
                var key = keyValuePair.Key;
                var value = keyValuePair.Value;
                writer.WriteStartElement(XmlConvert.EncodeLocalName(key));
                if (value != null)
                {
                    writer.WriteValue(value);
                }

                writer.WriteEndElement();
            }
        }
    }
}