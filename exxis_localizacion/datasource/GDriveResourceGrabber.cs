using exxis_localizacion.entidades;
using exxis_localizacion.google;
using exxis_localizacion.util;
using ExxisBibliotecaClases.entidades;
using ExxisBibliotecaClases.metodos;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.datasource
{
    public class GDriveResourceGrabber : IResourceGrabber
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);
        public SB1Package PaqueteActual { get; set; }
        public GDriveResourceGrabber()
        {
        }
        public List<TablaUsuarioWrp> GetUserTableList()
        {
            // ID TABLAS DE USUARIO : https://docs.google.com/spreadsheets/d/1JJgsy77bZ9fYg64knzV4i_RQOyBPPeHr1i0MkBp42UI/edit#gid=1019202745
            List<TablaUsuarioWrp> tablas = new List<TablaUsuarioWrp>();
            try
            {
                GoogleService google = new GoogleService();
                var service = google.getGoogle_Proxy_SheetService();
                string spreadsheetId = PaqueteActual.TablasUsuarioId; //"1JJgsy77bZ9fYg64knzV4i_RQOyBPPeHr1i0MkBp42UI";

                if (String.IsNullOrEmpty(spreadsheetId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado TABLAS DE USUARIO");
                }

                IList<IList<Object>> values = null;
                values = getValues(service, spreadsheetId, "UserTablesMD!A3:Z");

                foreach (IList<Object> fila in values)
                {
                    TablaUsuarioWrp utb = new TablaUsuarioWrp()
                    {
                        Accion = fila[3].ToString(),
                        O = new TablaUsuario()
                        {
                            Nombre = fila[0].ToString(),
                            Descripcion = fila[1].ToString(),
                            Tipo = (SAPbobsCOM.BoUTBTableType)Enum.Parse(typeof(SAPbobsCOM.BoUTBTableType), fila[2].ToString()),
                        }
                    };
                    tablas.Add(utb);
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetUserTableList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetUserTableList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
            return tablas;
        }
        public List<CampoUsuarioWrp> GetUserFieldList()
        {
            // ID CAMPOS DE USUARIO : https://docs.google.com/spreadsheets/d/1XnFUvDPZQjSRmY9dUMpyUjQ9qRt-x7th3ofP2FeH4qE/edit#gid=1340305812

            List<CampoUsuarioWrp> campos = new List<CampoUsuarioWrp>();

            try
            {
                GoogleService google = new GoogleService();
                var service = google.getGoogle_Proxy_SheetService();
                string spreadsheetId = PaqueteActual.CamposUsuarioId; //"1XnFUvDPZQjSRmY9dUMpyUjQ9qRt-x7th3ofP2FeH4qE";

                if (String.IsNullOrEmpty(spreadsheetId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado CAMPOS DE USUARIO");
                }

                IList<IList<object>> cabecera = getValues(service, spreadsheetId, "UserFieldsMD!A3:Z");
                IList<IList<object>> valoresvalidos = getValues(service, spreadsheetId, "ValidValues!A3:Z");

                foreach (IList<object> udf in cabecera)
                {
                    CampoUsuarioWrp campo = new CampoUsuarioWrp()
                    {
                        Accion = Convert.ToString(udf[12]).Trim(),
                        O = new CampoUsuario()
                        {
                            Tabla = Convert.ToString(udf[1]).Trim(),
                            Codigo = Convert.ToString(udf[2]).Trim(),
                            FieldId = Int32.Parse(Convert.ToString(udf[0]).Trim()),
                            Tipo = (SAPbobsCOM.BoFieldTypes)Enum.Parse(typeof(SAPbobsCOM.BoFieldTypes), Convert.ToString(udf[3]).Trim()),
                            Tamaño = (string.IsNullOrWhiteSpace(Convert.ToString(udf[4]).Trim()) ? 0 : Int32.Parse(Convert.ToString(udf[4]).Trim())),
                            Descripcion = Convert.ToString(udf[5]).Trim(),
                            SubTipo = string.IsNullOrWhiteSpace(udf[6].ToString()) ? SAPbobsCOM.BoFldSubTypes.st_None : (SAPbobsCOM.BoFldSubTypes)Enum.Parse(typeof(SAPbobsCOM.BoFldSubTypes), Convert.ToString(udf[6]).Trim()),
                            LinkedTable = Convert.ToString(udf[7]).Trim(),
                            DefaultValue = Convert.ToString(udf[8]).Trim(),
                            TamañoEditable = (string.IsNullOrWhiteSpace(Convert.ToString(udf[9]).Trim()) ? 0 : Int32.Parse(Convert.ToString(udf[9]).Trim())),
                            Mandatory = (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), udf[10].ToString(), true),
                            LinkedUDO = (udf.Count > 11 ? Convert.ToString(udf[11]).Trim() : ""),
                            ValoresValidos = new List<ValorValido>()
                        }
                    };

                    IList<IList<object>> vvlocal = valoresvalidos.Where(x => Convert.ToInt16(x[0]) == campo.O.FieldId &&
                                                                              Convert.ToString(x[1]).Trim().CompareTo(campo.O.Tabla) == 0)
                                                         .ToList();
                    foreach (IList<object> vv in vvlocal.OrderBy(x => x[3]))
                    {
                        campo.O.ValoresValidos.Add(
                            new ValorValido()
                            {
                                Valor = Convert.ToString(vv[3]).Trim(),
                                Descripcion = Convert.ToString(vv[4]).Trim()
                            }
                        );
                    }
                    campos.Add(campo);
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetUserFieldList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetUserFieldList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }

            return campos;
        }
        public List<ObjetoUsuarioWrp> GetUserObjectList()
        {
            // ID OBJETOS DE USUARIO https://docs.google.com/spreadsheets/d/1l4P-EocF1c7h2n45j5kejK9H4i8a6AOwwR3SEAeKSBw/edit#gid=1067009662

            List<ObjetoUsuarioWrp> lista = new List<ObjetoUsuarioWrp>();

            try
            {
                GoogleService google = new GoogleService();
                var service = google.getGoogle_Proxy_SheetService();

                string spreadsheetId = PaqueteActual.ObjetosUsuarioId; //"1l4P-EocF1c7h2n45j5kejK9H4i8a6AOwwR3SEAeKSBw";

                if (String.IsNullOrEmpty(spreadsheetId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado OBJETOS DE USUARIO");
                }

                //cabecera
                IList<IList<object>> oudo = getValues(service, spreadsheetId, "UserObjectsMD!A3:AZ");
                //childs
                IList<IList<object>> childs = getValues(service, spreadsheetId, "ChildTables!A3:Z");
                //findcols
                IList<IList<object>> findcols = getValues(service, spreadsheetId, "FindColumns!A3:Z");
                //formcols
                IList<IList<object>> formcols = getValues(service, spreadsheetId, "FormColumns!A3:Z");
                //enhformcols
                IList<IList<object>> enhformcols = getValues(service, spreadsheetId, "EnhancedFormColumns!A3:Z");

                foreach (IList<object> userobjectdef in oudo)
                {
                    string CodeObjeto = Convert.ToString(userobjectdef[0]).Trim();
                    string NameObjeto = Convert.ToString(userobjectdef[1]).Trim();
                    string TablaPadre = Convert.ToString(userobjectdef[2]).Trim();
                    string TablaLog = Convert.ToString(userobjectdef[3]).Trim();
                    string Tipo = Convert.ToString(userobjectdef[4]).Trim();
                    string ManageSeries = Convert.ToString(userobjectdef[5]).Trim();
                    string CanDelete = Convert.ToString(userobjectdef[6]).Trim();
                    string CanClose = Convert.ToString(userobjectdef[7]).Trim();
                    string CanCancel = Convert.ToString(userobjectdef[8]).Trim();
                    string ExtensionName = Convert.ToString(userobjectdef[9]).Trim();
                    string CanFind = Convert.ToString(userobjectdef[10]).Trim();
                    string CanYearTransfer = Convert.ToString(userobjectdef[11]).Trim();
                    string CanCreateDefaultForm = Convert.ToString(userobjectdef[12]).Trim();
                    string CanLog = Convert.ToString(userobjectdef[13]).Trim();
                    string OverwriteDllfile = Convert.ToString(userobjectdef[14]).Trim();
                    string UseUniqueFormType = Convert.ToString(userobjectdef[15]).Trim();
                    string CanArchive = Convert.ToString(userobjectdef[16]).Trim();
                    string Menu_Item = Convert.ToString(userobjectdef[17]).Trim();
                    string MenuCaption = Convert.ToString(userobjectdef[18]).Trim();
                    string FatherMenuID = Convert.ToString(userobjectdef[19]).Trim();
                    string Position = Convert.ToString(userobjectdef[20]).Trim();
                    string MenuUID = Convert.ToString(userobjectdef[24]).Trim();
                    string EnableEnhancedForm = Convert.ToString(userobjectdef[21]).Trim();
                    string RebuildEnhancedForm = Convert.ToString(userobjectdef[22]).Trim();
                    string FormSRF = Convert.ToString(userobjectdef[23]).Trim();
                    string CanApprove = Convert.ToString(userobjectdef[25]).Trim();
                    string TemplateID = (userobjectdef.Count > 26 ? Convert.ToString(userobjectdef[26]).Trim() : "");

                    ObjetoUsuarioWrp obj = new ObjetoUsuarioWrp() { O = new ObjetoUsuario() };
                    obj.Accion = Convert.ToString(userobjectdef[27]).Trim();
                    obj.O.CodeObjeto = CodeObjeto;
                    obj.O.NameObjeto = NameObjeto;
                    obj.O.TablaPadre = TablaPadre;
                    obj.O.Tipo = (SAPbobsCOM.BoUDOObjType)Enum.Parse(typeof(SAPbobsCOM.BoUDOObjType), Tipo);
                    if (!String.IsNullOrWhiteSpace(TablaLog)) obj.O.TablaLog = TablaLog;
                    if (!String.IsNullOrWhiteSpace(ManageSeries)) obj.O.ManageSeries = ManageSeries == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), ManageSeries);
                    if (!String.IsNullOrWhiteSpace(CanDelete)) obj.O.CanDelete = CanDelete == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), CanDelete);
                    if (!String.IsNullOrWhiteSpace(CanClose)) obj.O.CanClose = CanClose == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; // (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), CanClose);
                    if (!String.IsNullOrWhiteSpace(CanCancel)) obj.O.CanCancel = CanCancel == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), CanCancel);
                    if (!String.IsNullOrWhiteSpace(ExtensionName)) obj.O.ExtensionName = ExtensionName;
                    if (!String.IsNullOrWhiteSpace(CanFind)) obj.O.CanFind = CanFind == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), CanFind);
                    if (!String.IsNullOrWhiteSpace(CanYearTransfer)) obj.O.CanYearTransfer = CanYearTransfer == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; // (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), CanYearTransfer);
                    if (!String.IsNullOrWhiteSpace(CanCreateDefaultForm)) obj.O.CanCreateDefaultForm = CanCreateDefaultForm == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; // (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), CanCreateDefaultForm);
                    if (!String.IsNullOrWhiteSpace(CanLog)) obj.O.CanLog = CanLog == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; //SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), CanLog);
                    if (!String.IsNullOrWhiteSpace(OverwriteDllfile)) obj.O.OverwriteDllfile = OverwriteDllfile == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; // (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), OverwriteDllfile);
                    if (!String.IsNullOrWhiteSpace(UseUniqueFormType)) obj.O.UseUniqueFormType = UseUniqueFormType == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; // (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), UseUniqueFormType);
                    if (!String.IsNullOrWhiteSpace(CanArchive)) obj.O.CanArchive = CanArchive == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; // (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), CanArchive);
                    if (!String.IsNullOrWhiteSpace(Menu_Item) && Menu_Item.CompareTo("Y") == 0)
                    {
                        MenuItem mi = new MenuItem();
                        mi.Menu_Item = Menu_Item == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), Menu_Item);
                        if (!String.IsNullOrWhiteSpace(MenuCaption)) mi.MenuCaption = MenuCaption;
                        if (!String.IsNullOrWhiteSpace(FatherMenuID)) mi.FatherMenuID = int.Parse(FatherMenuID);
                        if (!String.IsNullOrWhiteSpace(Position)) mi.Position = int.Parse(Position);
                        if (!String.IsNullOrWhiteSpace(MenuUID)) mi.MenuUID = MenuUID;
                        obj.O.Menu_Item = mi;
                    }
                    if (!String.IsNullOrWhiteSpace(EnableEnhancedForm)) obj.O.EnableEnhancedForm = EnableEnhancedForm == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; // (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), EnableEnhancedForm);
                    if (!String.IsNullOrWhiteSpace(RebuildEnhancedForm)) obj.O.RebuildEnhancedForm = RebuildEnhancedForm == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; // (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), RebuildEnhancedForm);
                    if (!String.IsNullOrWhiteSpace(FormSRF)) obj.O.FormSRF = FormSRF;
                    if (!String.IsNullOrWhiteSpace(CanApprove)) obj.O.CanApprove = CanApprove == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO; //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), CanApprove);
                    if (!String.IsNullOrWhiteSpace(TemplateID)) obj.O.TemplateID = TemplateID;


                    //ChildTables
                    IList<IList<object>> childsLocal = childs.Where(x => Convert.ToString(x[0]).Trim().CompareTo(obj.O.CodeObjeto) == 0).ToList();
                    foreach (IList<object> child in childsLocal)
                    {
                        string tableName = Convert.ToString(child[2]).Trim();
                        string logtablename = Convert.ToString(child[3]).Trim();
                        string objectname = Convert.ToString(child[4]).Trim();

                        if (String.IsNullOrWhiteSpace(tableName)) continue;
                        obj.O.ChildTables.Add(new ChildTable()
                        {
                            TableName = tableName,
                            LogTableName = logtablename,
                            ObjectName = objectname
                        });
                    }

                    //FindCols
                    IList<IList<object>> findcolsLocal = findcols.Where(x => Convert.ToString(x[0]).Trim().CompareTo(obj.O.CodeObjeto) == 0).ToList();
                    foreach (IList<object> findcol in findcolsLocal)
                    {
                        String ColAlias2 = Convert.ToString(findcol[2]).Trim();
                        String ColumnDesc2 = Convert.ToString(findcol[3]).Trim();

                        if (String.IsNullOrWhiteSpace(ColAlias2)) continue;
                        obj.O.ListaUDO2.Add(new UDO2()
                        {
                            ColAlias = ColAlias2,
                            ColDesc = ColumnDesc2
                        });
                    }

                    //FormCols
                    IList<IList<object>> formcolsLocal = formcols.Where(x => Convert.ToString(x[0]).Trim().CompareTo(obj.O.CodeObjeto) == 0).ToList();
                    foreach (IList<object> formcol in formcolsLocal)
                    {
                        string sonNumber = Convert.ToString(formcol[2]).Trim();
                        string ColAlias3 = Convert.ToString(formcol[3]).Trim();
                        string ColDesc3 = Convert.ToString(formcol[4]).Trim();
                        string ColEdit3 = Convert.ToString(formcol[5]).Trim();

                        if (String.IsNullOrWhiteSpace(ColAlias3)) continue;
                        obj.O.ListaUDO3.Add(new UDO3()
                        {
                            ColAlias = ColAlias3,
                            ColDesc = ColDesc3,
                            CanEdit = ColEdit3 == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO, //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), ColEdit3),
                            SonNum = Int32.Parse(sonNumber)
                        });
                    }

                    //EnhancedFormCols
                    IList<IList<object>> enhformcolsLocal = enhformcols.Where(x => Convert.ToString(x[0]).Trim().CompareTo(obj.O.CodeObjeto) == 0).ToList();
                    foreach (IList<object> enhformcol in enhformcolsLocal)
                    {
                        string ColumnNum4 = Convert.ToString(enhformcol[2]).Trim();
                        string SonNum4 = Convert.ToString(enhformcol[3]).Trim();
                        string ColAlias4 = Convert.ToString(enhformcol[4]).Trim();
                        string ColDesc4 = Convert.ToString(enhformcol[5]).Trim();
                        string ColIsUsed4 = Convert.ToString(enhformcol[6]).Trim();
                        string ColEdit4 = Convert.ToString(enhformcol[7]).Trim();

                        if (String.IsNullOrWhiteSpace(ColAlias4)) continue;
                        obj.O.ListaUDO4.Add(new UDO4()
                        {
                            ColAlias = ColAlias4,
                            ColDesc = ColDesc4,
                            ColEdit = ColEdit4 == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO, //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), ColEdit4),
                            ColIsUsed = ColIsUsed4 == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO, // (SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), ColIsUsed4),
                            SonNum = Int32.Parse(SonNum4),
                            ColNum = Int32.Parse(ColumnNum4)
                        });
                    }

                    lista.Add(obj);
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetUserObjectList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetUserObjectList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
            return lista;
        }
        public List<CategoriaConsultaWrp> GetQueryCategoryList()
        {
            // ID GESTOR DE CONSULTAS: https://docs.google.com/spreadsheets/d/1cAvdZVeb04w2sM0hZ_j6v190vB44H9rynXFl34stIfY/edit#gid=543628153

            List<CategoriaConsultaWrp> lista = new List<CategoriaConsultaWrp>();
            try
            {
                GoogleService google = new GoogleService();
                var service = google.getGoogle_Proxy_SheetService();
                string spreadsheetId = PaqueteActual.GestorConsultasId; //"1cAvdZVeb04w2sM0hZ_j6v190vB44H9rynXFl34stIfY";

                if (String.IsNullOrEmpty(spreadsheetId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado CATEGORÍAS DE CONSULTAS");
                }

                IList<IList<object>> values = getValues(service, spreadsheetId, "QueryCategories!A3:Z");

                foreach (IList<object> categorydef in values)
                {
                    string Code = categorydef[0].ToString();
                    string CatName = categorydef[1].ToString();
                    string PermMask = categorydef[2].ToString();

                    CategoriaConsultaWrp ccw = new CategoriaConsultaWrp()
                    {
                        O = new CategoriaConsulta()
                        {
                            Code = long.Parse(Code),
                            Name = CatName,
                            Permissions = PermMask
                        }
                    };
                    lista.Add(ccw);
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetQueryCategoryList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetQueryCategoryList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
            return lista;
        }
        public List<ConsultaUsuarioWrp> GetUserQueryList(List<CategoriaConsultaWrp> listaCategoriaConsulta, string bdtype)
        {
            // ID GESTOR DE CONSULTAS: https://docs.google.com/spreadsheets/d/1cAvdZVeb04w2sM0hZ_j6v190vB44H9rynXFl34stIfY/edit#gid=543628153

            List<ConsultaUsuarioWrp> lista = new List<ConsultaUsuarioWrp>();
            try
            {
                GoogleService google = new GoogleService();
                var service = google.getGoogle_Proxy_SheetService();
                string spreadsheetId = PaqueteActual.GestorConsultasId; //"1cAvdZVeb04w2sM0hZ_j6v190vB44H9rynXFl34stIfY";

                if (String.IsNullOrEmpty(spreadsheetId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado CONSULTAS DE USUARIO");
                }

                IList<IList<object>> datos = getValues(service, spreadsheetId, "UserQueries!A3:Z");

                foreach (IList<object> uquerydef in datos)
                {
                    string qstring = Convert.ToString((bdtype.CompareTo("HANA") == 0 ? uquerydef[1] : uquerydef[0])).Trim();
                    string qname = Convert.ToString(uquerydef[2]).Trim();
                    string internalkey = Convert.ToString(uquerydef[3]).Trim();
                    string qtype = Convert.ToString(uquerydef[4]).Trim();
                    //string qcatname = Convert.ToString(uquerydef[5]).Trim();
                    long qcatid = long.Parse(Convert.ToString(uquerydef[5]).Trim());
                    string qcatname = listaCategoriaConsulta.Where(x => x.O.Code == qcatid).First().O.Name;


                    if (String.IsNullOrWhiteSpace(qstring.Trim()))
                    {
                        logger.Warn($"GetUserQueryList: Consulta \"{qcatname}\".\"{qname}\" no contiene una sentencia sql válida");
                        continue;
                    }

                    ConsultaUsuarioWrp cuw = new ConsultaUsuarioWrp()
                    {
                        Accion = Convert.ToString(uquerydef[6]).Trim(),
                        O = new ConsultaUsuario()
                        {
                            InternalKey = long.Parse(internalkey),
                            Query = qstring,
                            QueryDescription = qname,
                            QueryType = qtype == "W" ? SAPbobsCOM.UserQueryTypeEnum.uqtWizard :  //(SAPbobsCOM.UserQueryTypeEnum)Enum.Parse(typeof(SAPbobsCOM.UserQueryTypeEnum), qtype)                            
                                        qtype == "G" ? SAPbobsCOM.UserQueryTypeEnum.uqtGenerator :
                                        qtype == "S" ? SAPbobsCOM.UserQueryTypeEnum.uqtStoredProcedure : SAPbobsCOM.UserQueryTypeEnum.uqtRegular
                        }
                    };
                    //asignar consulta de categoría vinculada
                    CategoriaConsultaWrp ccw = null;
                    if (listaCategoriaConsulta != null && (ccw = listaCategoriaConsulta.FirstOrDefault(x => x.O.Code == qcatid)) != null)
                    {
                        cuw.O.CategoriaConsultaRef = ccw.O;
                    }
                    else
                    {
                        cuw.O.QueryCategory = qcatid;
                    }
                    lista.Add(cuw);
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetUserQueryList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetUserQueryList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
            return lista;
        }
        public List<BusquedaFormateadaWrp> GetFormattedSearchList(List<ConsultaUsuarioWrp> listaConsultaUsuario)
        {
            // ID GESTOR DE CONSULTAS: https://docs.google.com/spreadsheets/d/1cAvdZVeb04w2sM0hZ_j6v190vB44H9rynXFl34stIfY/edit#gid=543628153

            List<BusquedaFormateadaWrp> lista = new List<BusquedaFormateadaWrp>();

            try
            {
                GoogleService google = new GoogleService();
                var service = google.getGoogle_Proxy_SheetService();
                string spreadsheetId = PaqueteActual.GestorConsultasId; //"1cAvdZVeb04w2sM0hZ_j6v190vB44H9rynXFl34stIfY";

                if (String.IsNullOrEmpty(spreadsheetId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado BUSQUEDAS FORMATEADAS");
                }

                IList<IList<object>> datos = getValues(service, spreadsheetId, "FormattedSearches!A3:Z");
                if (datos != null)
                {
                    foreach (IList<object> frmtSrch in datos)
                    {
                        string formID = Convert.ToString(frmtSrch[1]).Trim();
                        string itemID = Convert.ToString(frmtSrch[2]).Trim();
                        string colID = Convert.ToString(frmtSrch[3]).Trim();
                        string action = Convert.ToString(frmtSrch[4]).Trim();
                        long qidlocal = long.Parse(Convert.ToString(frmtSrch[5]).Trim());
                        string refresh = Convert.ToString(frmtSrch[6]).Trim();
                        string FieldID = Convert.ToString(frmtSrch[7]).Trim();
                        string forceref = Convert.ToString(frmtSrch[8]).Trim();
                        string byField = Convert.ToString(frmtSrch[9]).Trim();
                        string qname = Convert.ToString(frmtSrch[10]).Trim();

                        BusquedaFormateadaWrp bfw = new BusquedaFormateadaWrp()
                        {
                            O = new BusquedaFormateada()
                            {
                                FormID = formID,
                                ItemID = itemID,
                                ColumnID = colID,
                                Action = action == "N" ? SAPbobsCOM.BoFormattedSearchActionEnum.bofsaNone : //(SAPbobsCOM.BoFormattedSearchActionEnum)Enum.Parse(typeof(SAPbobsCOM.BoFormattedSearchActionEnum), action),
                                         action == "Q" ? SAPbobsCOM.BoFormattedSearchActionEnum.bofsaQuery : SAPbobsCOM.BoFormattedSearchActionEnum.bofsaValidValues,
                                Refresh = refresh == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO, //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), refresh),
                                FieldID = FieldID,
                                ForceRefresh = forceref == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO, //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), forceref),
                                ByField = byField == "Y" ? SAPbobsCOM.BoYesNoEnum.tYES : SAPbobsCOM.BoYesNoEnum.tNO, //(SAPbobsCOM.BoYesNoEnum)Enum.Parse(typeof(SAPbobsCOM.BoYesNoEnum), byField)                            
                            }
                        };

                        ConsultaUsuarioWrp cuw = null;
                        if (listaConsultaUsuario != null && (cuw = listaConsultaUsuario.FirstOrDefault(x => x.O.InternalKey == qidlocal)) != null)
                        {
                            bfw.O.ConsultaUsuarioRef = cuw.O;
                        }
                        else
                        {
                            bfw.O.QueryID = qidlocal;
                        }

                        lista.Add(bfw);
                    }
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetFormattedSearchList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetFormattedSearchList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
            return lista;
        }
        public IDictionary<string, object> GetUserTablesData()
        {
            // ID Datos de tablas : https://docs.google.com/spreadsheets/d/1wiumnvddBHynlQtoUYDWSs4YqhCexVomYoI50l84Usg/edit#gid=0

            IDictionary<string, object> datos = new Dictionary<string, object>();

            try
            {
                GoogleService google = new GoogleService();
                var service = google.getGoogle_Proxy_SheetService();
                string spreadsheetId = PaqueteActual.DatosTablasUsuarioId; //"1wiumnvddBHynlQtoUYDWSs4YqhCexVomYoI50l84Usg";

                if (String.IsNullOrEmpty(spreadsheetId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado DATOS DE TABLAS DE USUARIO");
                }

                Spreadsheet sheet_metadata = service.Spreadsheets.Get(spreadsheetId).Execute();
                IList<Sheet> hojas = sheet_metadata.Sheets;

                foreach (Sheet hoja in hojas)
                {
                    String hojaNombre = hoja.Properties.Title;
                    if (hojaNombre.CompareTo("_") == 0) continue;
                    //Selecciona máximo rango (ZZ), la extracción de data sólo considerará el rango usado.
                    datos.Add(hojaNombre, getValues(service, spreadsheetId, $"{hojaNombre}!A:ZZ"));
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetUserTablesData: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetUserTablesData", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }

            return datos;
        }
        public List<DatosObjetoWrp> GetUserObjectsData(SAPbobsCOM.Company SB1Company)
        {
            // ID Datos de Objetos de usuario : https://docs.google.com/spreadsheets/d/1TZ_63vFi3fvQ98ZafxpBeUAcPD0Q0qUDx1WpJIBVWIU/edit#gid=1206145661

            List<DatosObjetoWrp> datosfinales = new List<DatosObjetoWrp>();

            try
            {
                GoogleService google = new GoogleService();
                var service = google.getGoogle_Proxy_SheetService();
                string spreadsheetId = PaqueteActual.DatosObjetosUsuarioId; //"1TZ_63vFi3fvQ98ZafxpBeUAcPD0Q0qUDx1WpJIBVWIU";

                if (String.IsNullOrEmpty(spreadsheetId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado DATOS DE OBJETOS DE USUARIO");
                }

                Spreadsheet sheet_metadata = service.Spreadsheets.Get(spreadsheetId).Execute();
                IList<Sheet> hojas = sheet_metadata.Sheets;

                //extracción de hojas de cálculo
                IDictionary<string, object> tablas = new Dictionary<string, object>();
                foreach (Sheet hoja in hojas)
                {
                    string hojaNombre = hoja.Properties.Title;
                    //Selecciona máximo rango (ZZ), la extracción de data sólo considerará el rango usado.
                    tablas.Add(hojaNombre, getValues(service, spreadsheetId, $"{hojaNombre}!A:ZZ"));
                }
                logger.Debug($"GetUserObjectsData: se han extraído {tablas.Count} hojas");

                foreach (KeyValuePair<string, object> tabla in tablas)
                {
                    IList<IList<object>> datostabla = (IList<IList<object>>)tabla.Value;

                    string codigo = tabla.Key;

                    if (datostabla == null || datostabla.Count == 0)
                    {
                        logger.Warn($"GetUserObjectsData: {codigo} no tiene datos para procesar");
                        continue;
                    }

                    SAPbobsCOM.IUserObjectsMD objectsMD = null;
                    try
                    {
                        objectsMD = (SAPbobsCOM.UserObjectsMD)SB1Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
                        if (!objectsMD.GetByKey(codigo))
                        {
                            logger.Warn($"GetUserObjectsData: Objeto \"{codigo}\" no ha sido encontrado en la sociedad actual");
                            continue;
                        }

                        //definir el campo llave según el tipo de objeto
                        String campollave = (objectsMD.ObjectType == SAPbobsCOM.BoUDOObjType.boud_MasterData ? "Code" : "DocEntry");

                        IList<IList<object>> datoscabecera = (IList<IList<object>>)tabla.Value;
                        IDictionary<string, object> tablashijas = new Dictionary<string, object>();

                        //extraer tablas hijas del objeto específico
                        SAPbobsCOM.UserObjectMD_ChildTables childtables = objectsMD.ChildTables;
                        for (int i = 0; i < childtables.Count; i++)
                        {
                            childtables.SetCurrentLine(i);
                            String tablahijanombre = childtables.TableName;
                            KeyValuePair<string, object> clave = tablas.FirstOrDefault(x => x.Key.CompareTo(tablahijanombre) == 0);
                            if (clave.Key == null) continue;
                            IList<IList<object>> childdata = (IList<IList<object>>)clave.Value;
                            tablashijas.Add(tablahijanombre, childdata);
                        }

                        DatosObjetoWrp dow = new DatosObjetoWrp() { O = new DatosObjeto() { Codigo = codigo, Tipo = objectsMD.ObjectType } };
                        //DATOS DE CABECERA
                        //se obvia las 2 primeras filas que contiene las cabeceras
                        for (int i = 2; i < datoscabecera.Count; i++)
                        {
                            IList<object> filacab = datoscabecera[0]; //nombres de columnas
                            int icampollave = filacab.IndexOf(campollave);

                            if (icampollave < 0)
                            {
                                throw new Exception($"No se ha encontrado el campo llave \"{campollave}\" en la hoja \"{codigo}\"");
                            }

                            string valorllave = datoscabecera[i][icampollave].ToString();

                            RegistroObjeto fila = new RegistroObjeto();
                            IList<object> filaSource = datoscabecera[i];
                            for (int j = 0; j < filacab.Count; j++)
                            {
                                string nombre = filacab[j].ToString();
                                object valor = (j < filaSource.Count ? filaSource[j] : null);
                                fila.Campos.Add(nombre, valor);
                            }
                            dow.O.Registros.Add(fila);

                            //DATOS DE DETALLES
                            ///Registrando datos de los hijos                    
                            foreach (KeyValuePair<string, object> tablahija in tablashijas)
                            {
                                DatosTabla dst = new DatosTabla() { Nombre = tablahija.Key };
                                List<IList<object>> datostablahijafull = ((IList<IList<object>>)tablahija.Value).ToList();

                                //se extrae lo regitros excepto la primera y segunda fila                        
                                IList<IList<object>> datostablahija = datostablahijafull.GetRange(2, datostablahijafull.Count - 2);
                                //se extrae la fila con los nombres de las columnas
                                IList<object> cabeceratablahija = datostablahijafull[0];

                                //buscar columna con campo llave en la primera fila donde estan las cabeceras
                                string campollavetablahija = "ParentKey";
                                int icampollavehijo = datostablahijafull[0].IndexOf(campollavetablahija);
                                if (icampollavehijo < 0)
                                {
                                    logger.Error($"GetUserObjectsData: La tabla \"{tablahija.Key}\" no contiene el campo \"{campollavetablahija}\". No se migrará");
                                    continue;
                                }

                                //filtrar registros de tablas hijas según registro de padre
                                datostablahija = datostablahija.Where(x => x[icampollavehijo].ToString().CompareTo(valorllave) == 0).ToList();
                                //agregando nuevos hijos
                                foreach (IList<object> campos in datostablahija)
                                {
                                    Dictionary<string, object> fila1 = new Dictionary<string, object>();
                                    for (int jc = 0; jc < cabeceratablahija.Count; jc++)
                                    {
                                        String nombre = cabeceratablahija[jc].ToString();
                                        if (nombre.CompareTo(campollavetablahija) == 0 || nombre.CompareTo("LineNum") == 0) continue;
                                        object valor = (jc < campos.Count ? campos[jc] : null);
                                        fila1.Add(nombre, valor);
                                    }
                                    dst.Registros.Add(fila1);
                                }
                                fila.TablasHijas.Add(dst);
                            }
                        }
                        datosfinales.Add(dow);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("GetUserObjectsData", ex);
                    }
                    finally
                    {
                        Common.LiberarObjeto(objectsMD);
                    }
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetUserObjectsData: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetUserObjectsData", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }

            return datosfinales;
        }
        private IList<IList<object>> getValues(SheetsService service, string spreadsheetId, string range)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request;
            request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;
            return values;
        }
        public List<FormatoElectronicoWrp> GetElectronicFormatList()
        {
            // ID CARPETA GEP : https://drive.google.com/drive/folders/1A-3w6-neVa_R2RJF8e5MuW41qODOPWrj

            List<FormatoElectronicoWrp> lista = new List<FormatoElectronicoWrp>();
            try
            {
                GoogleService google = new GoogleService();
                DriveService driveService = google.getGoogle_Proxy_Service();
                string folderId = PaqueteActual.FormatosElectronicosId; //"1A-3w6-neVa_R2RJF8e5MuW41qODOPWrj";

                if (String.IsNullOrEmpty(folderId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado FORMATOS ELECTRÓNICOS (GEP)");
                }

                String pageToken = null;
                do
                {
                    FilesResource.ListRequest listRequest = driveService.Files.List();
                    listRequest.Q = $"parents in '{folderId}'";// "mimeType='image /jpeg'";
                    listRequest.Spaces = "drive";
                    listRequest.Fields = "nextPageToken, files(id, name)";
                    listRequest.PageToken = pageToken;
                    FileList result = listRequest.Execute();

                    foreach (Google.Apis.Drive.v3.Data.File file in result.Files)
                    {
                        logger.Debug($"GetElectronicFormatList: descargando {file.Name} con Id = {file.Id}");
                        Stream stream = new MemoryStream();
                        driveService.Files.Get(file.Id).DownloadWithStatus(stream);
                        FormatoElectronico fe = GEPUtility.LoadFromStream(stream);
                        fe.FileName = file.Name;
                        lista.Add(new FormatoElectronicoWrp() { O = fe });
                    }
                    pageToken = result.NextPageToken;

                } while (pageToken != null);

            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetElectronicFormatList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetElectronicFormatList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
            return lista;
        }
        public List<FormatoCrystalWrp> GetCrystalFormatList(string bdtype)
        {
            // ID CARPETA RPT : https://drive.google.com/drive/folders/1TmvQqpG6U8VjQa-hQk5ixZ3ucofCSmAB

            List<FormatoCrystalWrp> lista = new List<FormatoCrystalWrp>();
            try
            {
                GoogleService google = new GoogleService();
                DriveService driveService = google.getGoogle_Proxy_Service();
                string folderId = PaqueteActual.ReportesCrystalId; //"1TmvQqpG6U8VjQa-hQk5ixZ3ucofCSmAB";

                if (String.IsNullOrEmpty(folderId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado REPORTES CRYSTAL");
                }


                FilesResource.ListRequest listRequest = driveService.Files.List();
                listRequest.Q = $"parents in '{folderId}' and mimeType = 'application/vnd.google-apps.folder' and name = '{bdtype}'";
                listRequest.Spaces = "drive";
                listRequest.Fields = "files(id, name)";
                FileList result = listRequest.Execute();

                if (result.Files.Count == 0)
                {
                    throw new Exception($"No se ha encontrado una carpeta denominada \"{bdtype}\" en el repositorio indicado ({folderId})");
                }

                Google.Apis.Drive.v3.Data.File folder = result.Files[0]; //carpeta de tipo base de datos
                logger.Debug($"GetCrystalFormatList: accediendo carpeta {folder.Name}");

                string rutamenu = "";
                /*
                 * La instanciación del contructor ReportDocument entra en conflicto con el servicio SAP B1 ElectronicFileFormatsService
                 * La lectura del título se hará en la importación
                 */
                //ReportDocument reportdocument = new ReportDocument();
                RecoletarFormatosCrystal(folder.Id, rutamenu, ref lista, listRequest, driveService);//, reportdocument);
                //Common.LiberarObjeto(reportdocument);

            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetCrystalFormatList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetCrystalFormatList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }

            return lista;
        }
        private void RecoletarFormatosCrystal(string idFolder, string rutamenu, ref List<FormatoCrystalWrp> lista, FilesResource.ListRequest listRequest, DriveService driveService)//, ReportDocument reportdocument)
        {
            string pageToken = null;
            do
            {
                listRequest.Q = $"parents in '{idFolder}'";
                listRequest.Spaces = "drive";
                listRequest.Fields = "nextPageToken, files(id, name, mimeType)";
                listRequest.PageToken = pageToken;
                FileList result = listRequest.Execute();

                foreach (Google.Apis.Drive.v3.Data.File file in result.Files)
                {
                    //Si es directorio
                    if (file.MimeType.CompareTo("application/vnd.google-apps.folder") == 0)
                    {
                        RecoletarFormatosCrystal(file.Id, $"{rutamenu}/{file.Name}", ref lista, listRequest, driveService);//, reportdocument);
                    }
                    else //Si es archivo
                    {
                        logger.Debug($"RecoletarFormatosCrystal: descargando {rutamenu}/{file.Name} con Id = {file.Id}");
                        Stream stream = new MemoryStream();
                        driveService.Files.Get(file.Id).DownloadWithStatus(stream);

                        string titulo = Path.GetFileNameWithoutExtension(file.Name);
                        /*
                         * La instanciación del contructor ReportDocument entra en conflicto con el servicio SAP B1 ElectronicFileFormatsService
                         * La lectura del título se hará en la importación
                            //crear carpeta
                            String dir = $"{Path.GetTempPath()}EX_LOC\\RPT"; ;
                            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                            //creando archivo temporal
                            string filetemp = $"{dir}\\{file.Name}";
                            using (FileStream fs = System.IO.File.Create(filetemp))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                stream.CopyTo(fs);
                            }
                            //cargar reporte
                            reportdocument.Load(filetemp);
                            titulo = reportdocument.SummaryInfo.ReportTitle;
                            //reportdocument.Dispose();
                            //reportdocument.Close();
                            if (System.IO.File.Exists(filetemp)) System.IO.File.Delete(filetemp);
                         */

                        lista.Add(new FormatoCrystalWrp()
                        {
                            O = new FormatoCrystal()
                            {
                                MenuPath = rutamenu,
                                FileName = file.Name,
                                Content = stream,
                                Title = titulo
                            }
                        });
                    }
                }
                pageToken = result.NextPageToken;

            } while (pageToken != null);
        }
        
        public List<Script> GetScriptListNew(string bdtype)
        {
            // ID CARPETA SCRIPT : https://drive.google.com/drive/folders/1ibPXPG8gZm-aP3demCQ5Dvu7zPa6AZZb

            List<Script> lista = new List<Script>();

            try
            {
                GoogleService google = new GoogleService();
                var service = google.getGoogle_Proxy_SheetService();
                string folderId = PaqueteActual.ScriptsId; //"1ibPXPG8gZm-aP3demCQ5Dvu7zPa6AZZb";

                if (String.IsNullOrEmpty(folderId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado SCRIPTS de procedimientos y funciones");
                }

                IList<IList<object>> datos = getValues(service, folderId, "Queries!A3:Z");

                foreach (IList<object> uquerydef in datos)
                {
                    string qstring = Convert.ToString((bdtype.CompareTo("HANA") == 0 ? uquerydef[1] : uquerydef[0])).Trim();
                    string qname = Convert.ToString(uquerydef[2]).Trim();
                    string qtype = Convert.ToString(uquerydef[3]).Trim();


                    if (String.IsNullOrWhiteSpace(qstring.Trim()))
                    {
                        logger.Warn($"GetScriptList: Script \"{qname}\" no contiene una sentencia sql válida");
                        continue;
                    }

                    Script cuw = new Script()
                    {
                        Accion = Convert.ToString(uquerydef[4]).Trim(),
                        O = new ConsultaUsuario()
                        {
                            Query = qstring,
                            QueryDescription = qname,
                            ScriptType = qtype
                        }
                    };

                    lista.Add(cuw);
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetScriptList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetScriptList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }

            return lista;
        }

        public List<ScriptFile> GetScriptList(string bdtype)
        {
            // ID CARPETA SCRIPT : https://drive.google.com/drive/folders/1ibPXPG8gZm-aP3demCQ5Dvu7zPa6AZZb

            List<ScriptFile> lista = new List<ScriptFile>();

            try
            {
                GoogleService google = new GoogleService();
                DriveService driveService = google.getGoogle_Proxy_Service();
                string folderId = PaqueteActual.ScriptsId; //"1ibPXPG8gZm-aP3demCQ5Dvu7zPa6AZZb";

                if (String.IsNullOrEmpty(folderId))
                {
                    throw new RecursoNoEncontradoException("No se han encontrado SCRIPTS");
                }


                FilesResource.ListRequest listRequest = driveService.Files.List();
                listRequest.Q = $"parents in '{folderId}' and mimeType != 'application/vnd.google-apps.folder' and name contains '{bdtype}'";
                listRequest.OrderBy = "name";
                listRequest.Spaces = "drive";
                listRequest.Fields = "files(id, name)";
                FileList result = listRequest.Execute();

                if (result.Files.Count == 0)
                {
                    throw new Exception($"No se ha encontrado una carpeta denominada \"{bdtype}\" en el repositorio indicado ({folderId})");
                }

                Google.Apis.Drive.v3.Data.File folder = result.Files[0]; //carpeta de tipo base de datos
                logger.Debug($"GetCrystalFormatList: accediendo carpeta {folder.Name}");

                string rutacarpeta = "";
                RecoletarScripts(folder.Id, rutacarpeta, ref lista, listRequest, driveService);

            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetScriptList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetScriptList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }

            return lista;
        }
        private void RecoletarScripts(string idFolder, string rutacarpeta, ref List<ScriptFile> lista, FilesResource.ListRequest listRequest, DriveService driveService)
        {
            string pageToken = null;
            do
            {
                listRequest.Q = $"parents in '{idFolder}'";
                listRequest.Spaces = "drive";
                listRequest.OrderBy = "name";
                listRequest.Fields = "nextPageToken, files(id, name, mimeType)";
                listRequest.PageToken = pageToken;
                FileList result = listRequest.Execute();

                foreach (Google.Apis.Drive.v3.Data.File file in result.Files)
                {
                    //Si es directorio
                    if (file.MimeType.CompareTo("application/vnd.google-apps.folder") == 0)
                    {
                        RecoletarScripts(file.Id, $"{rutacarpeta}/{file.Name}", ref lista, listRequest, driveService);
                    }
                    else //Si es archivo
                    {
                        logger.Debug($"RecoletarScripts: descargando {rutacarpeta}/{file.Name} con Id = {file.Id}");
                        Stream stream = new MemoryStream();
                        driveService.Files.Get(file.Id).DownloadWithStatus(stream);

                        string contenido = "";
                        stream.Position = 0;
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            contenido = sr.ReadToEnd();
                        }

                        lista.Add(new ScriptFile()
                        {
                            FolderPath = rutacarpeta,
                            FileName = file.Name,
                            Content = contenido
                        });
                    }
                }
                pageToken = result.NextPageToken;

            } while (pageToken != null);
        }
        public List<SB1Package> GetPackageList(string bdtype)
        {
            // ID CARPETA PAQUETES : https://drive.google.com/drive/folders/14Si4v7AuFVtDcffFhU1mBi4e3R3ErnVi

            try
            {
                GoogleService google = new GoogleService();
                DriveService driveService = google.getGoogle_Proxy_Service();
                string folderId = ConfigurationManager.AppSettings["FolderDrive"]; // "1erQSXhCUCTRJ0uCJmYktpTzVtvzXFvvL";
                List<SB1Package> lista = new List<SB1Package>();

                FilesResource.ListRequest listRequest = driveService.Files.List();
                listRequest.Q = $"parents in '{folderId}' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
                listRequest.Spaces = "drive";
                listRequest.Fields = "files(id, name)";
                FileList result = listRequest.Execute();

                if (result.Files.Count == 0)
                {
                    throw new Exception($"No se han encontrado paquetes disponibles en el repositorio indicado ({folderId})");
                }


                foreach (Google.Apis.Drive.v3.Data.File paqueteFolder in result.Files.OrderBy(x => x.Name))
                {
                    //analizar paquetes
                    listRequest.Q = $"parents in '{paqueteFolder.Id}' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
                    FileList resultfolders = listRequest.Execute();

                    logger.Debug($"GetPackageList: encontrado paquete '{paqueteFolder.Name}'");

                    if (resultfolders.Files.Count == 0)
                    {
                        logger.Warn($"GetPackageList: El paquete {paqueteFolder.Name}({paqueteFolder.Id}) está vacio");
                        continue;
                    }

                    SB1Package paquete = new SB1Package()
                    {
                        Nombre = paqueteFolder.Name,
                        Id = paqueteFolder.Id,
                        TipoBD = bdtype
                    };

                    foreach (Google.Apis.Drive.v3.Data.File elementoFolder in resultfolders.Files.OrderByDescending(x => x.Name))
                    {
                        switch (paqueteFolder.Name)
                        {
                            case "PLANTILLAS":
                                listRequest.Q = $"parents in '{elementoFolder.Id}' and mimeType != 'application/vnd.google-apps.folder' and trashed = false";
                                listRequest.Fields = "files(id, name, mimeType)";
                                FileList resultplantillas = listRequest.Execute();
                                foreach (Google.Apis.Drive.v3.Data.File plantilla in resultplantillas.Files.OrderBy(x => x.Name))
                                {
                                    //List<string> FileName = plantilla.Name.Split(' ').ToList();
                                    switch (plantilla.Name)
                                    {
                                        case "EXX_TABLAS_DE_USUARIO":
                                            paquete.TablasUsuarioId = plantilla.Id;
                                            break;
                                        case "EXX_CAMPOS_DE_USUARIO":
                                            paquete.CamposUsuarioId = plantilla.Id;
                                            break;
                                        case "EXX_OBJETOS_DE_USUARIO":
                                            paquete.ObjetosUsuarioId = plantilla.Id;
                                            break;
                                        case "EXX_DATOS_TABLAS":
                                            paquete.DatosTablasUsuarioId = plantilla.Id;
                                            break;
                                        case "EXX_DATOS_OBJETOS":
                                            paquete.DatosObjetosUsuarioId = plantilla.Id;
                                            break;
                                        case "EXX_GESTOR_CONSULTAS":
                                            paquete.GestorConsultasId = plantilla.Id;
                                            break;
                                        case "EXX_SCRIPTS":
                                            paquete.ScriptsId = plantilla.Id;
                                            break;
                                        default:
                                            switch (elementoFolder.Name)
                                            {
                                                case "08. Reportes":
                                                    paquete.ReportesCrystalId = elementoFolder.Id;
                                                    break;
                                                case "09. VISTAS":
                                                    if (bdtype.CompareTo("HANA") != 0) break;
                                                    listRequest.Q = $"parents in '{elementoFolder.Id}' and mimeType != 'application/vnd.google-apps.folder'";
                                                    FileList resultvistas = listRequest.Execute();
                                                    if (resultvistas.Files.Count != 0)
                                                    {
                                                        paquete.ModeloVistaId = resultvistas.Files[0].Id;
                                                    }
                                                    break;
                                                case "10. GEP":
                                                    paquete.FormatosElectronicosId = elementoFolder.Id;
                                                    break;
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }


                    lista.Add(paquete);
                }

                return lista;
            }
            catch (Exception ex)
            {
                logger.Error("GetPackageList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
        }
        public string GetModeloVistas()
        {
            string modeloVistasPath = "";

            try
            {
                GoogleService google = new GoogleService();
                DriveService driveService = google.getGoogle_Proxy_Service();
                string modelzipId = PaqueteActual.ModeloVistaId; //"1A-3w6-neVa_R2RJF8e5MuW41qODOPWrj";

                if (String.IsNullOrEmpty(modelzipId))
                {
                    throw new RecursoNoEncontradoException("No se ha encontrado el archivo de VISTAS (model.zip)");
                }

                logger.Debug($"GetModeloVistas: descargando model.zip con Id = {modelzipId}");

                FilesResource.GetRequest filerequest = driveService.Files.Get(modelzipId);
                Google.Apis.Drive.v3.Data.File file = filerequest.Execute();
                Stream stream = new MemoryStream();
                filerequest.DownloadWithStatus(stream);
                String dir = $"{Directory.GetCurrentDirectory()}\\paquetes\\{PaqueteActual.Nombre}\\vistas";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                modeloVistasPath = $"{dir}\\{file.Name}";
                using (FileStream fs = System.IO.File.Create(modeloVistasPath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fs);
                }
            }
            catch (RecursoNoEncontradoException ex)
            {
                logger.Info($"GetElectronictFormatsList: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Error("GetElectronictFormatsList", ex);
                throw ex;
            }
            finally
            {
                GC.Collect();
            }

            return modeloVistasPath;
        }
    }
}
