using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.entidades
{
    public class ResultadoGenerico : List<RegistroGenerico>
    {
        public string GetAsJSON()
        {
            string json = "[";
            foreach (RegistroGenerico reg in this)
            {
                json = $"{json}{reg.GetAsJSON()},";
            }
            if (json.EndsWith(","))
            {
                json = json.Substring(0, json.Length - 1);
            }
            return json + "]";
        }
    }
}
