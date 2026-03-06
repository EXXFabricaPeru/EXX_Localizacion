using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.entidades
{
    public class RegistroGenerico
    {
        public Dictionary<string, object> Campos { get; set; } = new Dictionary<string, object>();
        public string GetAsJSON()
        {
            string json = "{";
            foreach(KeyValuePair<string,object> campo in Campos)
            {
                json = $"{json}\"{campo.Key}\":\"{campo.Value}\",";
            }
            if (json.EndsWith(","))
            {
                json = json.Substring(0, json.Length - 1);
            }
            return json + "}";
        }
    }
}
