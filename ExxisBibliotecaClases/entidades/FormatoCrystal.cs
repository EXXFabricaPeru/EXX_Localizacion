using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ExxisBibliotecaClases.entidades
{
    public class FormatoCrystal
    {
        public string MenuPath { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public Stream Content { get; set; }
    }
}
