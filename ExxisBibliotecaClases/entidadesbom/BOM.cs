using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidadesbom
{
    [XmlRoot(ElementName = "BOM")]
    public class BOM:List<BO>
    {

    }
}
