using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.entidades
{
    public class QueryExecutedEventArgs
    {
        public string Mensaje { get; set; }
        public bool EsError { get; set; }
    }
}
