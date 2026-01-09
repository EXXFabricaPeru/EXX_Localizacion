using System.Xml.Serialization;

namespace ExxisBibliotecaClases.entidadesbom
{
    public class BOUserTablesMD:BO
    {
        public Header UserTablesMD { get; set; }
        public BOUserTablesMD()
        {
            AdmInfo = new AdmInfo("153");
        }
    }
}
