using ExxisBibliotecaClases.entidades;
using System.Collections.Generic;

namespace ExxisBibliotecaClases.entidadesbom
{
    public class BOUserFieldsMD : BO
    {
        public Header UserFieldsMD { get; set; }
        public List<row> ValidValues { get; set; }

        public BOUserFieldsMD()
        {
            AdmInfo = new AdmInfo("152");
        }
    }
}