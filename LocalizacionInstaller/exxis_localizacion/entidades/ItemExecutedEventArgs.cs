using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.entidades
{
    public class ItemExecutedEventArgs
    {
        public string Mensaje { get; set; }
        public bool EsError { get; set; }
        public ItemType Tipo { get; set; }
    }
}
