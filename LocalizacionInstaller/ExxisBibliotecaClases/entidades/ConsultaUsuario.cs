using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.entidades
{
    public class ConsultaUsuario
    {
        public string Query { get; set; }
        public string QueryDescription { get; set; }
        public long InternalKey { get; set; }
        public SAPbobsCOM.UserQueryTypeEnum QueryType { get; set; }
        public string ScriptType { get; set; }
        public long QueryCategory { 
            get { return CategoriaConsultaRef.Code; } 
            set { CategoriaConsultaRef.Code = value; } 
        }
        public CategoriaConsulta CategoriaConsultaRef { get; set; } = new CategoriaConsulta();
    }
}
