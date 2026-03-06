using ExxisBibliotecaClases.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidadesbom
{
    [XmlInclude(typeof(BOUserTablesMD))]
    [XmlInclude(typeof(BOUserFieldsMD))]
    [XmlInclude(typeof(BOUserObjectsMD))]
    public abstract class BO
    {
        public AdmInfo AdmInfo { get; set; }
    }
}
