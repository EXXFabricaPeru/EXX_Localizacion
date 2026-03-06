using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExxisBibliotecaClases.metodos
{
    public class SB1XmlWriter : XmlTextWriter
    {
        public SB1XmlWriter(TextWriter w)
                   : base(w) { }
        public SB1XmlWriter(Stream w, Encoding encoding)
                   : base(w, encoding) { }
        public SB1XmlWriter(string filename, Encoding encoding)
                   : base(filename, encoding) { }

        bool skip;

        public override void WriteStartAttribute(string prefix,
                                                 string localName,
                                                 string ns)
        {
            if (ns == "http://www.w3.org/2001/XMLSchema-instance" &&
                localName == "type")
            {
                skip = true;
            }
            else
            {
                base.WriteStartAttribute(prefix, localName, ns);
            }
        }

        public override void WriteString(string text)
        {
            if (!skip) base.WriteString(text);
        }

        public override void WriteEndAttribute()
        {
            if (!skip) base.WriteEndAttribute();
            skip = false;
        }
    }
}
