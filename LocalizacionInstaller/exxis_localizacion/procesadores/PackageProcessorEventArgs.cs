using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.procesadores
{
    public class PackageProcessorEventArgs
    {
        public double ProgresoGeneral { get; internal set; }
        public String TextoGeneral { get; set; }
        public String TextoParcial { get; set; }
        public double ProgresoParcial { get; set; }
        public bool Finalizado { get; internal set; }
        public string TextoResultadoParcial { get; internal set; }
        public string Vistas { get; set; }
        public override string ToString()
        {
            return $"ProgresoParcial = {ProgresoParcial}, ProgresoGeneral = {ProgresoGeneral}, Finalizado = {Finalizado}, TextoParcial = {TextoParcial}, TextoGeneral = {TextoGeneral}";
        }
    }
}
