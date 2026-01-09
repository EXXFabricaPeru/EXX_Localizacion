using ExxisBibliotecaClases.entidadesbom;
using ExxisBibliotecaClases.metodos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidades
{
    [XmlRoot(ElementName = "UserTablesMD")]
    public class TablaUsuario : SBOEntity
    {
        [XmlElement(ElementName = "TableName")]
        public string Nombre { get; set; }
        [XmlElement(ElementName = "TableDescription")]
        public string Descripcion { get; set; }
        [XmlElement(ElementName = "TableType")]

        public SAPbobsCOM.BoUTBTableType Tipo { get; set; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != obj.GetType()) {
                return false;
            }
            TablaUsuario tu = (TablaUsuario)obj;

            if (tu.Nombre != this.Nombre) {
                return false;
            }
            if (tu.Descripcion != this.Descripcion) {
                return false;
            }
            if (tu.Tipo != this.Tipo) {
                return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + this.Nombre.GetHashCode();
            hash = (hash * 7) + this.Descripcion.GetHashCode();
            hash = (hash * 7) + this.Tipo.GetHashCode();
            return hash;
        }
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
                bom.Add(new BOUserTablesMD(){
                    UserTablesMD = new Header() { row = this }
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
