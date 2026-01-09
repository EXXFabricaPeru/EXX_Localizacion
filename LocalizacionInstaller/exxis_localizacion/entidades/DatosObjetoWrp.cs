using ExxisBibliotecaClases.entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.entidades
{
    public class DatosObjetoWrp: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private RegistroObjeto _selected;
        public RegistroObjeto Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
            }
        }
        public bool Check { get; set; } = true;
        public DatosObjeto O { get; set; }
    }
}
