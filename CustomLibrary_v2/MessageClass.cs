using System;
using System.IO;
using System.Collections;

using System.Xml.Serialization;
using System.Text;

namespace CustomLibrary_v2
{
    [Serializable]
    public class MessageClass
    {

        public string receiver;
        public string sender;
        public string fileName;
        public string fullfileName;
        public byte[] container;
        public Symbols type;

        public MessageClass()
        {
            
        }

        public byte[] getBytes()
        {
            byte[] bytes;

            // передаем в конструктор тип класса Person
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(MessageClass));
            
            // получаем поток, куда будем записывать сериализованный объект
            using (var ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, this);
                bytes = ms.ToArray();
            }
            return bytes;
        }

        public static MessageClass getMessageFromBytes(byte[] bytes)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(MessageClass));
                MessageClass msg;
                // десериализуем объект
                using (var ms = new MemoryStream(bytes))
                {
                    object obj = xmlSerializer.Deserialize(ms);
                    msg = (MessageClass)obj;
                }
                return msg;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void formMessage(Symbols type,string receiver, string sender, string fileName, string fullFileName, string container)
        {
            this.type = type;
            this.receiver = receiver;
            this.sender = sender;
            this.fileName = fileName;
            this.fullfileName = fullFileName;
            this.container = Encoding.UTF8.GetBytes(container);
        }

    }
}
