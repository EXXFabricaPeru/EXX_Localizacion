using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.entidades
{
    public class RegistroObjeto
    {
        public Dictionary<string, object> Campos { get; set; } = new Dictionary<string, object>();
        public List<DatosTabla> TablasHijas { get; set; } = new List<DatosTabla>();
        public bool HasChilds { get { return TablasHijas.Count > 0; } set { } }
    }
}
