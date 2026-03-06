using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.entidades
{
    public class DatosObjeto
    {
        public string Codigo { get; set; }
        public SAPbobsCOM.BoUDOObjType Tipo { get; set; } 
        public List<RegistroObjeto> Registros { get; set; } = new List<RegistroObjeto>();
        public ObservableCollection<string> Columnas { 
            get 
            {
                if (Registros.Count == 0)
                {
                    return new ObservableCollection<string>();
                }
                else
                {
                    RegistroObjeto ro = Registros[0];
                    return new ObservableCollection<string>(ro.Campos.Select(x => x.Key));
                }
            } 
        }
    }
}
