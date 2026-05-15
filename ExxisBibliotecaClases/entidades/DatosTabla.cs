using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.entidades
{
    public class DatosTabla
    {
        public string Nombre { get; set; }
        public List<RegistroTabla> Registros { get; set; } = new List<RegistroTabla>();
    }
}
