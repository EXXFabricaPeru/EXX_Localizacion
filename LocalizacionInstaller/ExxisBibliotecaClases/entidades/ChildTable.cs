
using ExxisBibliotecaClases.entidadesbom;
using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidades
{
    public class ChildTable:row
    {
        public string LogTableName { get; set; }
        public string ObjectName { get; set; }
        public string TableName { get; set; }
    }
}