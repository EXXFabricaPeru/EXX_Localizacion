using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.entidades
{
    public class VerificandoEstructuraEventArgs
    {
        public List<String> Mensaje { get; set; } = new List<string>();
        public Int32 ProgresoGeneral { get; set; }
        public Int32 ProgresoParcial { get; set; }
        public bool ExitenErrores { get; internal set; }
    }
}
