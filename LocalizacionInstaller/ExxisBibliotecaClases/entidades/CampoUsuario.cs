using ExxisBibliotecaClases.entidadesbom;
using ExxisBibliotecaClases.metodos;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidades
{
    public class CampoUsuario:SBOEntity
    {
        [XmlElement(ElementName = "LinkedUDO")]
        public string LinkedUDO { get; set; }
        [XmlElement(ElementName = "Mandatory")]
        public BoYesNoEnum Mandatory { get; set; } = BoYesNoEnum.tNO;
        [XmlElement(ElementName = "DefaultValue")]
        public string DefaultValue { get; set; }
        [XmlElement(ElementName = "LinkedTable")]
        public string LinkedTable { get; set; }
        [XmlIgnore]
        public int FieldId { get; set; }
        [XmlElement(ElementName = "TableName")]
        public string Tabla { get; set; } = string.Empty;
        [XmlElement(ElementName = "Name")]
        public string Codigo { get; set; } = string.Empty;
        [XmlElement(ElementName = "Description")]
        public string Descripcion { get; set; } = string.Empty;
        [XmlElement(ElementName = "Type")]
        public BoFieldTypes Tipo { get; set; }
        [XmlElement(ElementName = "SubType")]
        public BoFldSubTypes SubTipo { get; set; } = BoFldSubTypes.st_None;
        [XmlElement(ElementName = "Size")]
        public int Tamaño { get; set; } = 0;
        [XmlElement(ElementName = "EditSize")]
        public int TamañoEditable { get; set; } = 0;
        [XmlIgnore]
        public List<ValorValido> ValoresValidos { get; set; }

        public override string GetAsXML()
        {
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();
            var xns = new XmlSerializerNamespaces();
            var serializer = new XmlSerializer(typeof(BOM));
            xns.Add(string.Empty, string.Empty);
            using (var stream = new StringWriter())
            using (var tr = new SB1XmlWriter(stream))
            {
                BOM bom = new BOM();
                bom.Add(new BOUserFieldsMD()
                {
                    UserFieldsMD = new Header() { row = this },
                    ValidValues = ValoresValidos.Cast<row>().ToList()
                });
                serializer.Serialize(tr, bom, xns);
                string xml = stream.ToString();
                //stopwatch.Stop();
                //Console.WriteLine("GetAsXML2: ha tomado {0} ms", stopwatch.ElapsedMilliseconds);
                return xml;
            }
        }
    }
}
