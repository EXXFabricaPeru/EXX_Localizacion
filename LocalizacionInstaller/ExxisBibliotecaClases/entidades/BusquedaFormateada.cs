namespace ExxisBibliotecaClases.entidades
{
    public class BusquedaFormateada
    {
        public long Index { get; set; }
        public string FormID { get; set; }
        public string ItemID { get; set; }
        public string ColumnID { get; set; }
        public SAPbobsCOM.BoFormattedSearchActionEnum Action { get; set; }
        public long QueryID {
            get { return ConsultaUsuarioRef.InternalKey; } 
            set { ConsultaUsuarioRef.InternalKey = value; }
        }
        public SAPbobsCOM.BoYesNoEnum Refresh { get; set; }
        public string FieldID { get; set; }
        public SAPbobsCOM.BoYesNoEnum ForceRefresh { get; set; }
        public SAPbobsCOM.BoYesNoEnum ByField { get; set; }
        public ConsultaUsuario ConsultaUsuarioRef { get; set; } = new ConsultaUsuario();
    }
}
