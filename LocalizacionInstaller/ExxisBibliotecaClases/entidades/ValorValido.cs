using ExxisBibliotecaClases.entidadesbom;
using System;
using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidades
{
    public class ValorValido: row
    {
        [XmlElement(ElementName = "Value")]
        public string Valor { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Descripcion { get; set; }
    }
}