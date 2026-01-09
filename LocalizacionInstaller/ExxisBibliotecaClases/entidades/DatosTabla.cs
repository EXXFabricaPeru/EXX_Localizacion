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
        public List<Dictionary<string, object>> Registros { get; set; } = new List<Dictionary<string, object>>();
        public ObservableCollection<string> Columnas
        {
            get
            {
                if (Registros.Count == 0)
                {
                    return new ObservableCollection<string>();
                }
                else
                {
                    return new ObservableCollection<string>(Registros[0].Keys.ToList());
                }
            }
        }
    }
}
