using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidades
{
    [XmlRoot(ElementName = "Resultados")]
    public class Resultados<T>
    {
        public T[] Registros { get; set; }
        public static Resultados<T> GetFromXML(string xml)
        {            
            var serializer = new XmlSerializer(typeof(Resultados<T>));
            using (var tr = new StringReader(xml))
            {
                return (Resultados<T>)serializer.Deserialize(tr);
            }
        }
    }
}
