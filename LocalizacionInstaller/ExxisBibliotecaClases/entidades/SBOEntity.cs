using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidades
{ 
    [XmlInclude(typeof(TablaUsuario))]
    [XmlInclude(typeof(CampoUsuario))]
    [XmlInclude(typeof(ObjetoUsuario))]
    public abstract class SBOEntity
    {
        public abstract string GetAsXML();
    }
}
