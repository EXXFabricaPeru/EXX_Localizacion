using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExxisBibliotecaClases.entidades;

namespace ExxisBibliotecaClases.entidadesbom
{
    public class BOUserObjectsMD:BO
    {
        public BOUserObjectsMD()
        {
            AdmInfo = new AdmInfo("206");
        }
        public Header UserObjectsMD { get; set; }
        public List<row> UserObjectMD_ChildTables { get; set; }
        public List<row> UserObjectMD_FindColumns { get; set; }
        public List<row> UserObjectMD_FormColumns { get; set; }
        public List<row> UserObjectMD_EnhancedFormColumns { get; set; }
    }
}
