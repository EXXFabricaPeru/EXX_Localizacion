using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.entidades
{
    public class ScriptFile
    {
        public bool Check { get; set; } = true;
        public string FileName { get; set; }
        public string Content { get; set; }
        public string FolderPath { get; set; }
    }
}
