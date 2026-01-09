namespace ExxisBibliotecaClases.entidadesbom
{
    public class AdmInfo
    {
        public string Object { get; set; }
        public string Version { get; set; }
        public AdmInfo()
        {

        }
        public AdmInfo(string pObject)
        {
            Version = "2";
            Object = pObject;
        }
        public AdmInfo(string pObject, string pVersion)
        {
            Version = pVersion;
            Object = pObject;
        }
    }
}