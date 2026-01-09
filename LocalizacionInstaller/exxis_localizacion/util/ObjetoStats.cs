using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.util
{
    public class ObjetoStats
    {
        public string IndicadorIcon { get; private set; } = "resources/imgs/red-light.png";
        public string GridZIndex { get; private set; } = "0";
        public string LoaderZIndex { get; private set; } = "1";
        public string ErrorZIndex { get; private set; } = "2";
        public string ErrorMensaje { get; private set; } = "Datos no disponibles";
        public void SetWatingMode()
        {
            IndicadorIcon = "resources/imgs/flashing.gif";
            GridZIndex = "0";
            ErrorZIndex = "0";
            LoaderZIndex = "1";
        }
        public void SetSucessMode()
        {
            IndicadorIcon = "resources/imgs/green-light.png";
            GridZIndex = "1";
            LoaderZIndex = "0";
            ErrorZIndex = "0";
        }
        public void SetErrorMode(string msj)
        {
            IndicadorIcon = "resources/imgs/red-light.png";
            ErrorZIndex = "1";
            GridZIndex = "0";
            LoaderZIndex = "0";
            ErrorMensaje = msj;
        }
        public bool Listo
        {
            get { return LoaderZIndex.CompareTo("0") == 0; }
        }
    }
}
