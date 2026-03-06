using ExxisBibliotecaClases.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidadesbom
{
    [XmlInclude(typeof(ValorValido))]
    [XmlInclude(typeof(ChildTable))]
    [XmlInclude(typeof(UDO2))]
    [XmlInclude(typeof(UDO3))]
    [XmlInclude(typeof(UDO4))]
    public abstract class row
    {

    }
}
