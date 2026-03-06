using ExxisBibliotecaClases.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.entidades
{
    public class ConsultaUsuarioWrp
    {
        public bool Check { get; set; } = true;
        public string Accion { get; set; }
        public ConsultaUsuario O { get; set; }
    }
}
