using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ExxisBibliotecaClases.entidadesbom;
using ExxisBibliotecaClases.metodos;
using SAPbobsCOM;

namespace ExxisBibliotecaClases.entidades
{
    public class ObjetoUsuario:SBOEntity
    {
        [XmlElement(ElementName = "TableName")]
        public string TablaPadre { get; set; } = string.Empty;
        [XmlIgnore]
        public string TablaHijo { get; set; } = string.Empty;
        [XmlElement(ElementName = "LogTableName")]
        public string TablaLog { get; set; } = string.Empty;
        [XmlElement(ElementName = "Code")]
        public string CodeObjeto { get; set; } = string.Empty;
        [XmlElement(ElementName = "Name")]
        public string NameObjeto { get; set; } = string.Empty;
        [XmlElement(ElementName = "FormSRF")]
        public string FormSRF { get; set; } = string.Empty;
        [XmlIgnore]
        public MenuItem Menu_Item { get; set; } = new MenuItem();
        [XmlElement(ElementName = "MenuItem")]
        public BoYesNoEnum MenuItem { get { return Menu_Item.Menu_Item; } set { Menu_Item.Menu_Item = value; }  }
        [XmlElement(ElementName = "MenuUID")]
        public string MenuUID { get { return Menu_Item.MenuUID; } set { Menu_Item.MenuUID = value; } }
        [XmlElement(ElementName = "MenuCaption")]
        public string MenuCaption { get { return Menu_Item.MenuCaption; } set { Menu_Item.MenuCaption = value; } }
        [XmlElement(ElementName = "FatherMenuID")]
        public int FatherMenuID { get { return Menu_Item.FatherMenuID; } set { Menu_Item.FatherMenuID = value; } }
        [XmlElement(ElementName = "Position")]
        public int Position { get { return Menu_Item.Position; } set { Menu_Item.Position = value; } }
        [XmlElement(ElementName = "ManageSeries")]
        public BoYesNoEnum ManageSeries { get; set; } = BoYesNoEnum.tNO;
        [XmlElement(ElementName = "EnableEnhancedForm")]
        public BoYesNoEnum EnableEnhancedForm { get; set; } = BoYesNoEnum.tYES;
        [XmlElement(ElementName = "RebuildEnhancedForm")]
        public BoYesNoEnum RebuildEnhancedForm { get; set; } = BoYesNoEnum.tNO;
        [XmlElement(ElementName = "ObjectType")]
        public BoUDOObjType Tipo { get; set; }
        [XmlElement(ElementName = "CanFind")]
        public BoYesNoEnum CanFind { get; set; } = BoYesNoEnum.tYES;
        [XmlElement(ElementName = "CanCancel")]
        public BoYesNoEnum CanCancel { get; set; } = BoYesNoEnum.tYES;
        [XmlElement(ElementName = "CanClose")]
        public BoYesNoEnum CanClose { get; set; } = BoYesNoEnum.tNO;
        [XmlElement(ElementName = "CanCreateDefaultForm")]
        public BoYesNoEnum CanCreateDefaultForm { get; set; } = BoYesNoEnum.tNO;
        [XmlElement(ElementName = "CanDelete")]
        public BoYesNoEnum CanDelete { get; set; } = BoYesNoEnum.tNO;
        [XmlElement(ElementName = "CanYearTransfer")]
        public BoYesNoEnum CanYearTransfer { get; set; } = BoYesNoEnum.tNO;
        [XmlElement(ElementName = "CanLog")]
        public BoYesNoEnum CanLog { get; set; } = BoYesNoEnum.tNO;
        [XmlElement(ElementName = "ExtensionName")]
        public string ExtensionName { get; set; }
        [XmlElement(ElementName = "OverwriteDllfile")]
        public BoYesNoEnum OverwriteDllfile { get; set; }
        [XmlElement(ElementName = "UseUniqueFormType")]
        public BoYesNoEnum UseUniqueFormType { get; set; }
        [XmlElement(ElementName = "CanArchive")]
        public BoYesNoEnum CanArchive { get; set; }
        [XmlElement(ElementName = "CanApprove")]
        public BoYesNoEnum CanApprove { get; set; }
        [XmlElement(ElementName = "TemplateID")]
        public string TemplateID { get; set; }
        [XmlIgnore]
        public List<ChildTable> ChildTables { get; set; } = new List<ChildTable>();
        [XmlIgnore]
        public List<UDO2> ListaUDO2 { get; set; } = new List<UDO2>();
        [XmlIgnore]
        public List<UDO3> ListaUDO3 { get; set; } = new List<UDO3>();
        [XmlIgnore]
        public List<UDO4> ListaUDO4 { get; set; } = new List<UDO4>();

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
                bom.Add(new BOUserObjectsMD()
                {
                    UserObjectsMD = new Header() { row = this },
                    UserObjectMD_ChildTables = ChildTables.Cast<row>().ToList(),
                    UserObjectMD_FindColumns = ListaUDO2.Cast<row>().ToList(),
                    UserObjectMD_FormColumns = ListaUDO3.Cast<row>().ToList(),
                    UserObjectMD_EnhancedFormColumns = ListaUDO4.Cast<row>().ToList()
                });
                serializer.Serialize(tr, bom, xns);
                string xml = stream.ToString();
                //stopwatch.Stop();
                //Console.WriteLine("GetAsXML2: ha tomado {0} ms", stopwatch.ElapsedMilliseconds);
                return xml;
            }
        }
    }

    public class MenuItem
    {
        public BoYesNoEnum Menu_Item { get; set; } = BoYesNoEnum.tNO;
        public int FatherMenuID { get; set; } = 0;
        public int Position { get; set; } = 0;
        public string MenuCaption { get; set; } = string.Empty;
        public string MenuUID { get; set; } = string.Empty;
    }

    public class UDO2 : row
    {
        [XmlElement(ElementName = "ColumnAlias")]
        public string ColAlias { get; set; } = string.Empty;
        [XmlElement(ElementName = "ColumnDescription")]
        public string ColDesc { get; set; } = string.Empty;
    }

    public class UDO3 : row
    {
        [XmlElement(ElementName = "SonNumber")]
        public int SonNum { get; set; } = 0;
        [XmlElement(ElementName = "FormColumnAlias")]
        public string ColAlias { get; set; } = string.Empty;
        [XmlElement(ElementName = "FormColumnDescription")]
        public string ColDesc { get; set; } = string.Empty;
        [XmlElement(ElementName = "Editable")]
        public BoYesNoEnum CanEdit { get; set; } = BoYesNoEnum.tNO;
    }

    public class UDO4 : row
    {
        [XmlElement(ElementName = "ChildNumber")]
        public int SonNum { get; set; } = 0;
        [XmlElement(ElementName = "ColumnNumber")]
        public int ColNum { get; set; } = 0;
        [XmlElement(ElementName = "ColumnAlias")]
        public string ColAlias { get; set; } = string.Empty;
        [XmlElement(ElementName = "ColumnDescription")]
        public string ColDesc { get; set; } = string.Empty;
        [XmlElement(ElementName = "Editable")]
        public BoYesNoEnum ColEdit { get; set; } = BoYesNoEnum.tNO;
        [XmlElement(ElementName = "ColumnIsUsed")]
        public BoYesNoEnum ColIsUsed { get; set; } = BoYesNoEnum.tNO;
    }
}
