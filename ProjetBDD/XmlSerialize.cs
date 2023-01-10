using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
namespace ProjetBDD
{
    public class XmlSerialize
    {
        public void ConvertListOfObjectsToXml<T>(List<T> list, string file, bool overwrite)
        {
            
            bool append = !overwrite;
            using (StreamWriter writer = new StreamWriter(file, append, Encoding.UTF8))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
                serializer.Serialize(writer, list);
            }
        }
    }
}
