using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.util
{
    public class RecursoNoEncontradoException:Exception
    {
        public RecursoNoEncontradoException(string mensaje):base(mensaje)
        {

        }
    }
}
