using CrystalDecisions.CrystalReports.Engine;
using exxis_localizacion.entidades;
using exxis_localizacion.Properties;
using ExxisBibliotecaClases;
using ExxisBibliotecaClases.entidades;
using ExxisBibliotecaClases.metodos;
using ExxisBibliotecaClases.query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace exxis_localizacion.dataccess
{
    public class SB1DataAccess
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        private MetadataManager _metadataManager;

        public event EventHandler<ItemExecutedEventArgs> ItemExecuted;
        private string _executingItemIdentifier = "";
        public SAPbobsCOM.Company SB1Company { get; private set; }
        public SAPbouiCOM.Application SB1Application { get; private set; }

        private static string cnnString = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056";
        private string newMenuName = "";

        public SB1DataAccess()
        {

        }
        public bool AddUserTable(TablaUsuario usertabledef, out string errorMessage, string Accion)
        {
            errorMessage = "";
            try
            {
                string xml = usertabledef.GetAsXML();
                string mensaje = _metadataManager.UserTableMDManager.CheckFromXMLString(xml, 0, Accion);
                mensaje = string.IsNullOrEmpty(mensaje) ? "OK" : "mensaje";
                logger.Info($"AddUserTable: {mensaje}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Tabla '{usertabledef.Nombre}' => {ex.Message}";
                logger.Error($"AddUserTable: {errorMessage}", ex);
                return false;
            }
        }
        public bool AddUserField(CampoUsuario userfielddef, out string errorMessage, string Accion)
        {
            errorMessage = "";
            try
            {
                string xml = userfielddef.GetAsXML();
                string mensaje = _metadataManager.UserFieldsManager.CheckFromXMLString(xml, 0, Accion);
                mensaje = string.IsNullOrEmpty(mensaje) ? "OK" : "mensaje";
                logger.Info($"AddUserField: {mensaje}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"[ERROR] Campo \"{userfielddef.Tabla}\".\"{userfielddef.Codigo}\" => {ex.Message}";
                logger.Error($"AddUserField: {errorMessage}", ex);
                return false;
            }
        }
        public bool AddUserObject(ObjetoUsuario userobjectdef, out string errorMessage, string Accion)
        {
            errorMessage = "";

            try
            {
                string xml = userobjectdef.GetAsXML();
                string mensaje = _metadataManager.UserObjectManager.CheckFromXMLString(xml, 0, Accion);
                mensaje = string.IsNullOrEmpty(mensaje) ? "OK" : "mensaje";
                logger.Info($"AddUserObject: {mensaje}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"[ERROR] Objeto '{userobjectdef.CodeObjeto}' => {ex.Message}";
                logger.Error($"AddUserObject: {errorMessage}", ex);
                return false;
            }
        }

        public bool AddScriptNew(Script scriptfile, out string errorMessage)
        {
            errorMessage = "";

            try
            {
                _executingItemIdentifier = $"Script {scriptfile.O.QueryDescription}\"";
                logger.Debug($"AddScript: cargando {_executingItemIdentifier}");
                QueryFactory qf;

                try
                {
                    string query = string.Empty;
                    if (scriptfile.O.ScriptType == "S")
                        query = $"DROP PROCEDURE \"{scriptfile.O.QueryDescription}\"";
                    else
                        query = $"DROP FUNCTION \"{scriptfile.O.QueryDescription.ToUpper()}\"";

                    qf = new QueryFactory(SB1Company, query);
                    qf.QueryExecuted += Qf_QueryExecuted;
                    qf.ExecuteUpdate();
                }
                catch (Exception ex)
                {
                }

                logger.Info($"DeleteScript: {_executingItemIdentifier} finalizado con éxito");
                if (scriptfile.Accion != "R")
                {
                    qf = new QueryFactory(SB1Company, scriptfile.O.Query);
                    qf.QueryExecuted += Qf_QueryExecuted;
                    qf.ExecuteUpdate();
                    logger.Info($"AddScript: {_executingItemIdentifier} finalizado con éxito");
                }



                return true;
            }
            catch (Exception ex)
            {
                string errmsg = $"[ERROR] {_executingItemIdentifier}";
                errorMessage = $"{errmsg}\": {ex.Message}";
                logger.Error($"AddScript: {errmsg}", ex);
                return false;
            }
        }

        public bool AddScript(ScriptFile scriptfile, out string errorMessage)
        {
            errorMessage = "";

            try
            {
                _executingItemIdentifier = $"Script \"{scriptfile.FolderPath}/{scriptfile.FileName}\"";
                logger.Debug($"AddScript: cargando {_executingItemIdentifier}");

                QueryFactory qf = new QueryFactory(SB1Company, scriptfile.Content);
                qf.QueryExecuted += Qf_QueryExecuted;
                qf.ExecuteUpdate();

                logger.Info($"AddScript: {_executingItemIdentifier} finalizado con éxito");

                return true;
            }
            catch (Exception ex)
            {
                string errmsg = $"[ERROR] {_executingItemIdentifier}";
                errorMessage = $"{errmsg}\": {ex.Message}";
                logger.Error($"AddScript: {errmsg}", ex);
                return false;
            }
        }
        private void Qf_QueryExecuted(object sender, QueryExecutedEventArgs e)
        {
            ItemExecutedEventArgs ie = new ItemExecutedEventArgs();
            string etiqueta = e.EsError ? "[ERROR] " : "[INFO] ";
            ie.Mensaje = $"{etiqueta}{_executingItemIdentifier}: {e.Mensaje}";
            ie.EsError = e.EsError;
            ie.Tipo = ItemType.Script;
            ItemExecuted?.Invoke(this, ie);
        }
        public bool AddCrystalFormat(FormatoCrystal rpt, out string errorMessage)
        {
            errorMessage = "";

            SAPbobsCOM.ReportLayout newReport = null;
            try
            {
                logger.Debug($"AddCrystalFormat: cargando \"{rpt.MenuPath}/{rpt.Title}\"");

                //extraer título. Desactivado hasta que se resuelva problema con dll
                //rpt.Title = ExtractReportTitle(rpt);

                //extraer menú id o crearlo.
                string menupadreuid = GetOrCreateMenuUID(rpt.MenuPath);

                QueryFactory qf = new QueryFactory(SB1Company, "GetReportMenuInfo", "resources.queries");
                qf.SetString("menuPadreGUID", "35");
                qf.SetString("titulo", rpt.Title);
                List<RegistroGenerico> regs = qf.ExecuteQuery();

                SAPbobsCOM.ReportLayoutsService rptService = (SAPbobsCOM.ReportLayoutsService)SB1Company.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.ReportLayoutsService);
                SAPbobsCOM.ReportLayoutParams newReportParam = null;

                SAPbobsCOM.BlobParams oBlobParams = (SAPbobsCOM.BlobParams)SB1Company.GetCompanyService().GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlobParams);
                oBlobParams.Table = "RDOC";
                oBlobParams.Field = "Template";
                SAPbobsCOM.BlobTableKeySegment oKeySegment = oBlobParams.BlobTableKeySegments.Add();
                oKeySegment.Name = "DocCode";

                if (regs.Count == 0)
                { // el título no fue encontrado
                    newReport = (SAPbobsCOM.ReportLayout)rptService.GetDataInterface(SAPbobsCOM.ReportLayoutsServiceDataInterfaces.rlsdiReportLayout);
                    newReport.Author = SB1Company.UserName;
                    newReport.Category = SAPbobsCOM.ReportLayoutCategoryEnum.rlcCrystal;
                    newReport.Name = rpt.Title;
                    newReport.TypeCode = "RCRI";
                    newReportParam = rptService.AddReportLayoutToMenu(newReport, menupadreuid);

                    oKeySegment.Value = newReportParam.LayoutCode;
                }
                else
                {
                    RegistroGenerico rg = regs[0];
                    if (regs.Count > 1) // se encontró mas de 1 título.
                    {
                        logger.Warn($"AddCrystalFormat: se encontró mas de 1 reporte con título \"{rpt.Title}\". Se procede a tomar el primer registro");
                        ResultadoGenerico resgen = new ResultadoGenerico();
                        resgen.AddRange(regs);
                        logger.Warn($"AddCrystalFormat: resultado de registros = {resgen.GetAsJSON()}");
                    }

                    string menupath = rg.Campos["Ruta"].ToString();
                    //el reporte existe en un menu diferente al que se desea migrar.
                    if (menupath.CompareTo(rpt.MenuPath) != 0)
                    {
                        logger.Warn($"AddCrystalFormat: el reporte fue encontrado en el menu \"{menupath}\" distinto a \"{rpt.MenuPath}\"");
                        errorMessage = $"[WARN] El reporte \"{rpt.Title}\" ya existe. Se actualizará en su actual menu \"{menupath}\"";
                    }

                    string doccode = rg.Campos["Codigo"].ToString();

                    oKeySegment.Value = doccode;
                }

                int size = (int)rpt.Content.Length;
                byte[] buf = new byte[size];
                rpt.Content.Read(buf, 0, size);
                rpt.Content.Dispose();

                SAPbobsCOM.Blob oBlob = (SAPbobsCOM.Blob)SB1Company.GetCompanyService().GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlob);
                oBlob.Content = Convert.ToBase64String(buf, 0, size);

                SB1Company.GetCompanyService().SetBlob(oBlobParams, oBlob);

                logger.Info($"AddCrystalFormat: {rpt.Title} importado con éxito");

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"[ERROR] Reporte crystal \"{rpt.FileName}\" => {ex.Message}";
                logger.Error($"AddCrystalFormat: {errorMessage}", ex);
                return false;
            }
            finally
            {
                Common.LiberarObjeto(newReport);
            }
        }
        public bool UserObjectExists(string code)
        {
            SAPbobsCOM.UserObjectsMD udomd = null;
            try
            {
                udomd = (SAPbobsCOM.UserObjectsMD)SB1Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
                return udomd.GetByKey(code);
            }
            catch (Exception ex)
            {
                logger.Error($"UserObjectExists", ex);
                throw new Exception("UserObjectExists: " + ex.Message);
            }
            finally
            {
                Common.LiberarObjeto(udomd);
            }
        }
        private string ExtractReportTitle(FormatoCrystal rpt)
        {
            string tempfilePath = Path.GetTempFileName();
            string titulo = rpt.Title;
            ReportDocument rd = null;
            try
            {
                rd = new ReportDocument();
                using (FileStream fs = File.Create(tempfilePath))
                {
                    rpt.Content.Seek(0, SeekOrigin.Begin);
                    rpt.Content.CopyTo(fs);
                }
                rd.Load(tempfilePath);
                titulo = rd.SummaryInfo.ReportTitle;
                if (string.IsNullOrWhiteSpace(titulo))
                {
                    titulo = Path.GetFileNameWithoutExtension(rpt.FileName);
                    logger.Warn($"ExtractReportTitle: el título del reporte es vacio, se asigna el nombre de archivo de reporte: \"{titulo}\"");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ExtractReportTitle", ex);
            }
            finally
            {
                if (File.Exists(tempfilePath)) File.Delete(tempfilePath);
                if (rd != null) rd.Close();
            }
            return titulo;
        }
        private string GetOrCreateMenuUID(string menuPath)
        {
            string[] menuparts = menuPath.Split('/');
            string menupadre = "43531"; //Informes financieros
            string qstr = Properties.Resources.getMenuInfo;
            QueryFactory qf = new QueryFactory(SB1Company, qstr);
            List<string> menubc = new List<string>();
            menubc.Add("|Financials|Finanzas|");
            menubc.Add("|Financial Reports|Informes financieros|");
            foreach (string menupart in menuparts)
            {
                if (string.IsNullOrWhiteSpace(menupart)) continue;
                qf.SetString("parentmenuuid", menupadre);
                qf.SetString("menuname", menupart);
                List<RegistroGenerico> lrg = qf.ExecuteQuery();

                menubc.Add($"|{menupart}|");
                //el sub-menu existe
                if (lrg.Count > 0)
                {
                    menupadre = lrg[0].Campos["MenuUID"].ToString();
                }
                //el sub-menu NO existe. se intenta crear
                else
                {
                    if (SB1Application == null)
                    {
                        throw new Exception($"No se ha encontrado parte de la ruta de menú \"Finanzas/Informes Financieros/{menuPath}\". Inicie sesión conectandose a un cliente SAP Business One y vuelva a intentarlo");
                    }
                    //abrir formulario Estructura de menú
                    string menuStructureUid = "30337";
                    SB1Application.ActivateMenuItem(menuStructureUid);
                    //capturar formulario Estructura de menú
                    SAPbouiCOM.Form frm = SB1Application.Forms.ActiveForm;
                    //capturar Matrix item
                    SAPbouiCOM.Item matrixItem = frm.Items.Item("410000005");
                    //convertir a Matrix
                    SAPbouiCOM.Matrix matrix = (SAPbouiCOM.Matrix)matrixItem.Specific;
                    //asilar única columna
                    SAPbouiCOM.Column listbox = matrix.Columns.Item("410000001");

                    int vrca = matrix.VisualRowCount;
                    int rango = matrix.VisualRowCount;
                    int ini = 1;
                    foreach (string menuitem in menubc)
                    {
                        bool found = false;
                        for (int i = ini; i <= ini + rango - 1; i++)
                        {
                            SAPbouiCOM.EditText et = (SAPbouiCOM.EditText)listbox.Cells.Item(i).Specific;
                            if (menuitem.Contains($"|{et.String}|"))
                            {
                                //click sobre fila de menu
                                listbox.Cells.Item(i).Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                                listbox.Cells.Item(i).Click(SAPbouiCOM.BoCellClickType.ct_Collapsed);
                                rango = matrix.VisualRowCount - vrca;
                                vrca = matrix.VisualRowCount;
                                ini = i + 1;
                                found = true;
                                break;
                            }
                        }

                        //no fue encontrado. Simula agregación
                        if (!found)
                        {
                            //listbox.Cells.Item(ipadre).Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                            SAPbouiCOM.Item btnNew = frm.Items.Item("410000007");
                            newMenuName = menupart;
                            //lanzar ventana de creación de menú y evento de formload.
                            btnNew.Click();
                            //finalizado el control regresa a este punto

                            //get newly created menuuid. se utilizan los mismos parámetros previos.
                            lrg = qf.ExecuteQuery();
                            if (lrg.Count == 0)
                            {
                                throw new Exception($"Error creando menú {menupart} debajo de {menupadre}");
                            }
                            menupadre = lrg[0].Campos["MenuUID"].ToString();
                            logger.Info($"GetOrCreateMenuUID: se ha creado el menú \"{menupart}\" con id = {menupadre}");
                        }
                    }
                    //cerrar formulario
                    frm.Close();
                }
            }
            return menupadre;
        }
        public bool AddUserTableData(string tabla, IList<IList<object>> datos, ref List<string> errorMessages)
        {
            SAPbobsCOM.UserTable oUserTable = null;
            try
            {
                try
                {
                    oUserTable = SB1Company.UserTables.Item(tabla);
                }
                catch (Exception ex)
                {
                    logger.Warn($"AddUserTableData: tabla \"{tabla}\" no existe. {ex.Message}");
                    return true;
                }

                //DATOS DE CABECERA
                //se obvia las 2 primeras filas que contiene las cabeceras
                for (int i = 2; i < datos.Count; i++)
                {
                    IList<object> filacab = datos[0];
                    int icampollave = filacab.IndexOf("Code");

                    if (icampollave < 0)
                    {
                        throw new Exception($"No se ha encontrado el campo llave \"Code\" en la hoja \"{tabla}\"");
                    }

                    String valorllave = datos[i][icampollave].ToString();

                    bool existe = oUserTable.GetByKey(valorllave);

                    IList<object> filas = datos[i];
                    for (int j = 0; j < filas.Count; j++)
                    {
                        String nombre = datos[0][j].ToString();
                        object valor = filas[j];
                        switch (nombre)
                        {
                            case "Code":
                                oUserTable.Code = valor.ToString();
                                break;
                            case "Name":
                                oUserTable.Name = valor.ToString();
                                break;
                            default:
                                oUserTable.UserFields.Fields.Item(nombre).Value = valor;
                                break;
                        }
                    }

                    string errorMessage = "";
                    int ret = 0;

                    if (!existe)
                    {
                        ret = oUserTable.Add();
                        errorMessage = "insertado con éxito";
                    }
                    else
                    {
                        ret = oUserTable.Update();
                        errorMessage = "actualizado con éxito";
                    }

                    if (ret == 0)
                    {
                        logger.Info($"AddUserTableData: Registro {tabla}[{valorllave}] {errorMessage}");
                    }
                    else
                    {
                        errorMessage = $"[ERROR] Registro {tabla}[{valorllave}] {SB1Company.GetLastErrorDescription()}";
                        errorMessages.Add(errorMessage);
                        logger.Error($"AddUserTableData: {errorMessage}");
                    }
                }
                return (errorMessages.Count == 0);
            }
            catch (Exception ex)
            {
                logger.Error("AddUserTableData", ex);
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(oUserTable);
            }
        }
        public bool AddUserObjectData(DatosObjeto datosobj, ref List<string> errorMessages)
        {
            SAPbobsCOM.GeneralService objService = null;
            try
            {
                objService = SB1Company.GetCompanyService().GetGeneralService(datosobj.Codigo);

                string campollave = (datosobj.Tipo == SAPbobsCOM.BoUDOObjType.boud_Document ? "DocEntry" : "Code");

                //DATOS DE CABECERA
                foreach (RegistroObjeto regObj in datosobj.Registros)
                {
                    KeyValuePair<string, object> regllave = regObj.Campos.FirstOrDefault(x => x.Key.CompareTo(campollave) == 0);

                    String valorllave = regllave.Value.ToString();

                    SAPbobsCOM.GeneralDataParams oCabParam = (SAPbobsCOM.GeneralDataParams)objService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams);

                    oCabParam.SetProperty(campollave, valorllave);

                    SAPbobsCOM.GeneralData oCab = null;

                    bool existe = false;
                    try
                    {
                        oCab = objService.GetByParams(oCabParam);
                        existe = true;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"AddUserObjectData: Registro {datosobj.Codigo}[{valorllave}] no fue encontrado. Se creará uno nuevo. {ex.Message}");
                        oCab = (SAPbobsCOM.GeneralData)objService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);
                    }

                    string Accion = string.Empty;
                    //Asignnado campos de CABECERA
                    foreach (KeyValuePair<string, object> campo in regObj.Campos)
                    {
                        if (campo.Value != null && !string.IsNullOrWhiteSpace(campo.Value.ToString()))
                        {
                            if (campo.Key == "Accion")
                                Accion = (string)campo.Value;
                            else
                                oCab.SetProperty(campo.Key, campo.Value);
                        }
                    }



                    //Asignando TABLAS HIJAS
                    foreach (DatosTabla childtable in regObj.TablasHijas)
                    {
                        SAPbobsCOM.GeneralDataCollection oChild = oCab.Child(childtable.Nombre);

                        //Eliminando registros existentes
                        while (oChild.Count > 0) oChild.Remove(0);

                        //Asignnado campos de DETALLE
                        foreach (Dictionary<string, object> campos in childtable.Registros)
                        {
                            SAPbobsCOM.GeneralData gdChild = oChild.Add();
                            foreach (KeyValuePair<string, object> campo in campos)
                            {
                                if (campo.Value != null && !string.IsNullOrWhiteSpace(campo.Value.ToString()))
                                    if (campo.Key != "Accion")
                                        gdChild.SetProperty(campo.Key, campo.Value);
                            }
                        }
                    }

                    string errorMessage = "";
                    try
                    {
                        if (Accion == "R")
                        {
                            errorMessage = "No se pudo eliminar";
                            objService.Delete(oCabParam);
                            errorMessage = "eliminado con éxito";
                        }
                        else
                        {
                            if (!existe)
                            {
                                errorMessage = "No se pudo agregar";
                                objService.Add(oCab);
                                errorMessage = "agregado con éxito";
                            }
                            else
                            {
                                errorMessage = "No se pudo actualizar";
                                objService.Update(oCab);
                                errorMessage = "actualizado con éxito";
                            }
                        }
                        logger.Info($"AddUserObjectData: Registro {datosobj.Codigo}[{valorllave}] {errorMessage}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"AddUserObjectData: XML = > {oCab.ToXMLString()}");
                        errorMessage = $"[ERROR] Registro {datosobj.Codigo}[{valorllave}] {errorMessage} : {ex.Message}";
                        errorMessages.Add(errorMessage);
                        logger.Error($"AddUserObjectData: {errorMessage}");
                    }
                }
                return (errorMessages.Count == 0);
            }
            catch (Exception ex)
            {
                logger.Error("AddUserObjectData", ex);
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(objService);
            }
        }
        public bool AddFormattedSearch(ref BusquedaFormateada frmtSrch, out string errorMessage)
        {
            errorMessage = "";
            int iRet = -1;

            SAPbobsCOM.FormattedSearches oFormattedSearches = null;
            try
            {
                oFormattedSearches = (SAPbobsCOM.FormattedSearches)SB1Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oFormattedSearches);

                int fsid = getFormattedSearchId(frmtSrch.FormID, frmtSrch.ItemID, frmtSrch.ColumnID);

                bool existe = oFormattedSearches.GetByKey(fsid);

                oFormattedSearches.FormID = frmtSrch.FormID;
                oFormattedSearches.ItemID = frmtSrch.ItemID;
                oFormattedSearches.ColumnID = frmtSrch.ColumnID;
                oFormattedSearches.Action = frmtSrch.Action;
                oFormattedSearches.Refresh = frmtSrch.Refresh;
                oFormattedSearches.FieldID = frmtSrch.FieldID;
                oFormattedSearches.ForceRefresh = frmtSrch.ForceRefresh;
                oFormattedSearches.ByField = frmtSrch.ByField;
                oFormattedSearches.QueryID = (int)frmtSrch.QueryID;
                if (!existe)
                {
                    errorMessage = $"Busqueda Formateada \"{frmtSrch.FormID}\".\"{frmtSrch.ItemID}\".\"{frmtSrch.ColumnID}\" creada correctamente";
                    iRet = oFormattedSearches.Add();
                }
                else
                {
                    errorMessage = $"Busqueda Formateada \"{frmtSrch.FormID}\".\"{frmtSrch.ItemID}\".\"{frmtSrch.ColumnID}\" actualizada correctamente";
                    iRet = oFormattedSearches.Update();
                }

                if (iRet == 0)
                {
                    logger.Info($"AddFormattedSearch: {errorMessage}");
                    return true;
                }
                else
                {
                    errorMessage = $"[ERROR] Busqueda Formateada \"{frmtSrch.FormID}\".\"{frmtSrch.ItemID}\".\"{frmtSrch.ColumnID}\" => ({iRet}) {SB1Company.GetLastErrorDescription()}";
                    logger.Error($"AddFormattedSearch: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("AddFormattedSearch", ex);
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(oFormattedSearches);
            }
        }
        private int getFormattedSearchId(string formID, string itemID, string colID)
        {
            String qry = "select COALESCE(MAX(\"IndexID\"),-1) AS \"IndexID\" from \"CSHS\" WHERE \"FormID\" = ${formid} AND \"ItemID\" = ${itemid} AND \"ColID\" = ${colid}";
            QueryFactory qf = new QueryFactory(SB1Company, qry);
            qf.SetString("formid", formID);
            qf.SetString("itemid", itemID);
            qf.SetString("colid", colID);

            int fieldid = qf.ExecuteSingleResult<int>();

            return fieldid;
        }
        public bool AddUserQuery(ref ConsultaUsuario uquerydef, out string errorMessage, string Accion)
        {
            errorMessage = "";
            int iRet = -1;

            SAPbobsCOM.UserQueries oUserQueries = null;
            try
            {
                bool ishana = (SB1Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB);

                oUserQueries = (SAPbobsCOM.UserQueries)SB1Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserQueries);
                //string qcatname = Convert.ToString(uquerydef[5]).Trim();
                //int qcatid = getCategoryId(qcatname);
                int qid = getQueryId(uquerydef.QueryDescription, uquerydef.QueryCategory);

                if (String.IsNullOrWhiteSpace(uquerydef.Query.Trim()))
                {
                    errorMessage = $"[ERROR] Consulta \"{uquerydef.CategoriaConsultaRef.Name}\".\"{uquerydef.QueryDescription}\" => No contiene una sentencia sql válida";
                    logger.Warn($"AddUserQuery: {errorMessage}");
                    return true;
                }

                bool existe = oUserQueries.GetByKey(qid, (int)uquerydef.QueryCategory);

                oUserQueries.Query = uquerydef.Query;
                oUserQueries.QueryDescription = uquerydef.QueryDescription;
                oUserQueries.QueryType = uquerydef.QueryType;
                oUserQueries.QueryCategory = (int)uquerydef.QueryCategory;

                if (Accion == "R")
                {
                    if (!existe) iRet = 0;
                    else
                    {
                        errorMessage = $"Query \"{uquerydef.CategoriaConsultaRef.Name}\".\"{uquerydef.QueryDescription}\" actualizada correctamente";
                        iRet = oUserQueries.Remove();
                    }
                }
                else
                {
                    if (!existe)
                    {
                        errorMessage = $"Query \"{uquerydef.CategoriaConsultaRef.Name}\".\"{uquerydef.QueryDescription}\" creada correctamente";
                        iRet = oUserQueries.Add();
                        string nuevaLlave = SB1Company.GetNewObjectKey();
                        string[] llaves = nuevaLlave.Split('\t');
                        logger.Debug($"AddUserQuery: nueva llave = {nuevaLlave}, llave = {llaves[0]}");
                        uquerydef.InternalKey = long.Parse(llaves[0]);
                    }
                    else
                    {
                        errorMessage = $"Query \"{uquerydef.CategoriaConsultaRef.Name}\".\"{uquerydef.QueryDescription}\" actualizada correctamente";
                        iRet = oUserQueries.Update();
                        uquerydef.InternalKey = qid;
                    }
                }

                if (iRet == 0)
                {
                    if (Accion == "R")
                    {
                        logger.Info($"DeleteUserQuery: {errorMessage}");
                        return true;
                    }
                    else
                    {
                        logger.Info($"AddUserQuery: {errorMessage}");
                        return true;
                    }
                }
                else
                {
                    errorMessage = $"[ERROR] Consulta \"{uquerydef.CategoriaConsultaRef.Name}\".\"{uquerydef.QueryDescription}\" => ({iRet}) {SB1Company.GetLastErrorDescription()}";
                    logger.Error($"AddUserQuery: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("AddUserQuery", ex);
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(oUserQueries);
            }
        }
        private int getQueryId(string qname, long qcatid)
        {
            String qry = "SELECT COALESCE(MAX(\"IntrnalKey\"),-1000)  AS \"IntrnalKey\" FROM \"OUQR\" WHERE \"QName\"=${nombre} AND \"QCategory\"=${categoriaid}";
            QueryFactory qf = new QueryFactory(SB1Company, qry);
            qf.SetString("nombre", qname);
            qf.SetInt("categoriaid", qcatid);

            int fieldid = qf.ExecuteSingleResult<int>();

            return fieldid;
        }
        public bool AddUserQueryCategory(ref CategoriaConsulta categorydef, out string errorMessage)
        {
            errorMessage = "";
            int iRet = -1;

            SAPbobsCOM.QueryCategories oQueryCategory = null;
            try
            {
                oQueryCategory = (SAPbobsCOM.QueryCategories)SB1Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQueryCategories);

                int idCat = getCategoryId(categorydef.Name);

                bool existe = oQueryCategory.GetByKey(idCat);

                oQueryCategory.Name = categorydef.Name;
                oQueryCategory.Permissions = categorydef.Permissions;
                if (!existe)
                {
                    errorMessage = $"Categoría '{categorydef.Name}' creada correctamente";
                    iRet = oQueryCategory.Add();
                    categorydef.Code = long.Parse(SB1Company.GetNewObjectKey());
                }
                else
                {
                    errorMessage = $"Categoría '{categorydef.Name}' actualizado correctamente";
                    iRet = oQueryCategory.Update();
                    categorydef.Code = idCat;
                }

                if (iRet == 0)
                {
                    logger.Info($"AddUserQueryCategory: {errorMessage}");
                    return true;
                }
                else
                {
                    errorMessage = $"[ERROR] Categoría '{categorydef.Name}' => ({iRet}) {SB1Company.GetLastErrorDescription()}";
                    logger.Error($"AddUserQueryCategory: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("AddUserQueryCategory", ex);
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(oQueryCategory);
            }
        }
        private int getCategoryId(string catName)
        {
            String qry = "SELECT COALESCE(MAX(\"CategoryId\"),-1000)  AS \"CategoryId\" FROM \"OQCN\" WHERE \"CatName\"=${nombre}";
            QueryFactory qf = new QueryFactory(SB1Company, qry);
            qf.SetString("nombre", catName);

            int fieldid = qf.ExecuteSingleResult<int>();

            return fieldid;
        }
        private int getFieldId(string tableName, string fieldname)
        {
            String qry = "SELECT COALESCE(MAX(\"FieldID\"),-1)  AS \"FieldID\" FROM \"CUFD\" WHERE \"TableID\"=${tabla} AND \"AliasID\"=${campo}";
            QueryFactory qf = new QueryFactory(SB1Company, qry);
            qf.SetString("tabla", tableName);
            qf.SetString("campo", fieldname);

            int fieldid = qf.ExecuteSingleResult<int>();

            return fieldid;
        }
        public bool AddElectronicFormat(FormatoElectronico gep, out String errorMessage)
        {
            SAPbobsCOM.ElectronicFileFormatParams effp = null;
            SAPbobsCOM.ImportFileParam ifp = null;
            SAPbobsCOM.ElectronicFileFormatsService effs = (SAPbobsCOM.ElectronicFileFormatsService)SB1Company.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.ElectronicFileFormatsService);
            errorMessage = "";
            string tempfilePath = Path.GetTempFileName();
            string accion = "";
            try
            {
                errorMessage = "AddElectronicFormat " + gep.Name;
                logger.Debug($"AddElectronicFormat: Cargando => {gep.Name}({gep.FileName})");

                QueryFactory qf = new QueryFactory(SB1Company, Resources.getElectronicFileFormat);
                qf.SetString("effName", gep.Name);
                int idformat = qf.ExecuteSingleResult<int>();

                if (idformat > 0)
                {
                    logger.Debug($"AddElectronicFormat: Encontrado {gep.Name}");
                    accion = "Eliminando";
                    effp = (SAPbobsCOM.ElectronicFileFormatParams)effs.GetDataInterface(SAPbobsCOM.ElectronicFileFormatsServiceDataInterfaces.effsElectronicFileFormatParams);
                    effp.Name = gep.Name;
                    effs.DeleteElectronicFileFormat(effp);
                    logger.Debug($"AddElectronicFormat: Eliminado con éxito");
                }
                else
                {
                    logger.Debug($"AddElectronicFormat: No fue encontrado");
                }

                using (FileStream fs = File.Create(tempfilePath))
                {
                    gep.Content.Seek(0, SeekOrigin.Begin);
                    gep.Content.CopyTo(fs);
                    gep.Content.Close();
                }
                accion = "Creando";
                ifp = (SAPbobsCOM.ImportFileParam)effs.GetDataInterface(SAPbobsCOM.ElectronicFileFormatsServiceDataInterfaces.effsImportFileParam);
                ifp.FilePath = tempfilePath;
                effs.AddElectronicFileFormat(ifp);
                logger.Info($"AddElectronicFormat: Creado con éxito");

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"[ERROR] {gep.Name}: No pudo ser cargado. Elimine manualmente Formato Electrónico y vuelva a intentarlo";
                logger.Error($"AddElectronicFormat: {accion}", ex);
                return false;
            }
            finally
            {
                Common.LiberarObjeto(effp);
                Common.LiberarObjeto(ifp);
                if (File.Exists(tempfilePath)) File.Delete(tempfilePath);
            }
        }
        public bool ConectarUI()
        {
            try
            {
                SAPbouiCOM.SboGuiApi sb1gui = new SAPbouiCOM.SboGuiApi();
                sb1gui.Connect(cnnString);

                SB1Application = sb1gui.GetApplication();

                SB1Application.ItemEvent += SB1Application_ItemEvent;
                Common.AgregarFiltroSeguro(SB1Application, SAPbouiCOM.BoEventTypes.et_FORM_LOAD, "4001");

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("ConectarUI", ex);
                return false;
            }
        }
        private void SB1Application_ItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if (pVal.BeforeAction || pVal.EventType != SAPbouiCOM.BoEventTypes.et_FORM_LOAD) return;
                //logger.Debug($"SB1Application_ItemEvent: FormUID = {FormUID}, EventType = {Enum.GetName(typeof(SAPbouiCOM.BoEventTypes), pVal.EventType)}, FormType={pVal.FormTypeEx}, BeforeAction = {pVal.BeforeAction}, InnerEvent = {pVal.InnerEvent}, ItemUID = {pVal.ItemUID}, FormMode = {Enum.GetName(typeof(SAPbouiCOM.BoFormMode), pVal.FormMode)}");
                if (pVal.FormTypeEx == "4001" && !string.IsNullOrWhiteSpace(newMenuName))
                {
                    //capturando dialog de nombre
                    SAPbouiCOM.Form dlg = SB1Application.Forms.Item(FormUID);
                    //asignar nombre de nuevo menu
                    SAPbouiCOM.EditText edt = (SAPbouiCOM.EditText)dlg.Items.Item("4").Specific;
                    edt.Value = newMenuName;
                    //aceptar nuevo nombre
                    dlg.Items.Item("1").Click();
                    newMenuName = "";
                }
                //controlando ventana de aviso de cambio de estructura de base de datos.
                if (pVal.FormTypeEx == "0")
                {
                    SAPbouiCOM.Form dlg = SB1Application.Forms.Item(FormUID);
                    if (!dlg.IsSystem || !dlg.Modal) return;
                    try
                    {
                        SAPbouiCOM.StaticText label7 = (SAPbouiCOM.StaticText)dlg.Items.Item("7").Specific;
                        SAPbouiCOM.StaticText label1000001 = (SAPbouiCOM.StaticText)dlg.Items.Item("1000001").Specific;
                        if ((label7.Caption.CompareTo("The database structure has been modified. In order to resume the process, all open") == 0 &&
                            label1000001.Caption.CompareTo("windows will be closed. Do you want to continue adding the user-defined field?") == 0) ||
                            (label7.Caption.CompareTo("Se ha modificado la estructura de la base de datos. Para reanudar el proceso se cerrarán") == 0 &&
                            label1000001.Caption.CompareTo("todas las ventanas abiertas. ¿Desea continuar añadiendo el campo definido p.usuario?") == 0))
                        {
                            logger.Info($"SB1Application_ItemEvent: Cerrando formulario de cambios en bd");
                            dlg.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"SB1Application_ItemEvent: Buscando formulario de cambios en bd = {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("SB1Application_ItemEvent", ex);
            }
        }
        public bool ConectarDI()
        {
            try
            {
                if (SetConnectionContext() == 0)
                {
                    SB1Company.language = SAPbobsCOM.BoSuppLangs.ln_Spanish_La;
                    int res = SB1Company.Connect();
                    if (res != 0)
                    {
                        logger.Error($"ConectarDI: ({SB1Company.GetLastErrorCode()}) - {SB1Company.GetLastErrorDescription()}");
                        return false;
                    }
                    logger.Debug($"ConectarDI: idioma detectado '{SB1Company.language}'");
                    if (SB1Company.language != SAPbobsCOM.BoSuppLangs.ln_Spanish_La &&
                        SB1Company.language != SAPbobsCOM.BoSuppLangs.ln_English)
                    {
                        throw new Exception("Sólo se acepta el idioma Español Latino o Ingles. Ajuste la configuración de su cliente SAP Business One.");
                    }
                    _metadataManager = new MetadataManager(SB1Company);
                }
                else
                {
                    string msj = $"({SB1Company.GetLastErrorCode()}) - {SB1Company.GetLastErrorDescription()}";
                    logger.Error($"ConectarDI: {msj}");
                    throw new Exception(msj);
                }
            }
            catch (Exception ex)
            {
                logger.Error("ConectarDI", ex);
                return false;
            }
            return true;
        }
        private int SetConnectionContext()
        {
            int setConnectionContextReturn;
            string sCookie;
            string sConnectionContext;

            SB1Company = new SAPbobsCOM.Company();
            sCookie = SB1Company.GetContextCookie();
            sConnectionContext = SB1Application.Company.GetConnectionContext(sCookie);
            if (SB1Company.Connected == true)
            {
                SB1Company.Disconnect();
            }
            setConnectionContextReturn = SB1Company.SetSboLoginContext(sConnectionContext);
            return setConnectionContextReturn;
        }
    }
}
