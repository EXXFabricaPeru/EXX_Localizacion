using System;
using SAPbobsCOM;
using ExxisBibliotecaClases.entidades;

namespace ExxisBibliotecaClases
{
    public class EstructuraSAP
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(EstructuraSAP));
        private Company oCompany;

        public EstructuraSAP(Company oCmpy)
        {
            oCompany = oCmpy;
        }

        public bool CrearTablaUsuario(TablaUsuario tUsuario, out string error)
        {
            error = string.Empty;
            UserTablesMD oUserTablesMD = null;
            try
            {
                oUserTablesMD = (UserTablesMD)oCompany.GetBusinessObject(BoObjectTypes.oUserTables);
                oUserTablesMD.TableName = tUsuario.Nombre;
                oUserTablesMD.TableDescription = tUsuario.Descripcion;
                oUserTablesMD.TableType = tUsuario.Tipo;
                if (oUserTablesMD.Add() != 0)
                {
                    throw new Exception(oCompany.GetLastErrorDescription());
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                logger.Error(ex.Message, ex);
            }
            finally
            {
                if (oUserTablesMD != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserTablesMD);
                oUserTablesMD = null;
                GC.Collect();
            }
            return false;
        }

        public bool CrearCampoUsuario(CampoUsuario cUsuario, out string error)
        {
            error = string.Empty;
            UserFieldsMD oUserField = null;
            try
            {
                oUserField = (UserFieldsMD)oCompany.GetBusinessObject(BoObjectTypes.oUserFields);
                oUserField.TableName = cUsuario.Tabla;
                oUserField.Name = cUsuario.Codigo;
                oUserField.Description = cUsuario.Descripcion;
                oUserField.Type = cUsuario.Tipo;
                if (cUsuario.SubTipo != BoFldSubTypes.st_None)
                {
                    oUserField.SubType = cUsuario.SubTipo;
                }
                if (cUsuario.Tamaño != 0)
                {
                    oUserField.Size = cUsuario.Tamaño;
                    oUserField.EditSize = cUsuario.TamañoEditable;
                }
                if (oUserField.Add() != 0)
                {
                    throw new Exception(oCompany.GetLastErrorDescription());
                }
                else
                {
                    logger.Debug($"CrearCampoUsuario: {cUsuario.Tabla}.{cUsuario.Codigo} creado con éxito");
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                logger.Error("CrearCampoUsuario", ex);
            }
            finally
            {
                if (oUserField != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserField);
                oUserField = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            return false;
        }

        public bool ValidarTablaCreada(string Tabla)
        {
            Recordset oRecordSet = null;
            try
            {
                oRecordSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = string.Empty;

                query = $"SELECT COUNT(0) FROM OUTB WHERE \"TableName\" = '{ Tabla }'";
                oRecordSet.DoQuery(query);
                return oRecordSet.Fields.Item(0).Value.ToString() != "0";
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
            finally
            {
                if (oRecordSet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);
                oRecordSet = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            return false;
        }

        public bool ValidarCampoCreado(CampoUsuario cUsuario)
        {
            Recordset oRecordSet = null;
            try
            {
                oRecordSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = string.Empty;

                query = $"SELECT COUNT(0) FROM CUFD WHERE \"TableID\" = '{ cUsuario.Tabla }' AND \"AliasID\" = '{ cUsuario.Codigo }'";
                oRecordSet.DoQuery(query);
                return oRecordSet.Fields.Item(0).Value.ToString() != "0";
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
            finally
            {
                if (oRecordSet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);
                oRecordSet = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            return false;
        }

        public bool ValidarObjetoCreado(ObjetoUsuario oUDO)
        {
            Recordset oRecordSet = null;
            try
            {
                oRecordSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = string.Empty;

                query = $"SELECT COUNT(0) FROM OUDO WHERE \"Code\" = '{ oUDO.CodeObjeto }'";
                oRecordSet.DoQuery(query);
                return oRecordSet.Fields.Item(0).Value.ToString() != "0";
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
            finally
            {
                if (oRecordSet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);
                oRecordSet = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            return false;
        }

        public bool CrearUDO(ObjetoUsuario objetoUsuario, out string err)
        {
            err = string.Empty;
            UserObjectsMD oUserObjectsMD = null;
            try
            {
                oUserObjectsMD = (UserObjectsMD)oCompany.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                /*
                
                oUserObjectsMD.ExtensionName = string.Empty;
                oUserObjectsMD.CanFind = objetoUsuario.CanFind;
                oUserObjectsMD.CanYearTransfer = objetoUsuario.CanYearTransfer;
                oUserObjectsMD.CanLog = objetoUsuario.CanLog;
                oUserObjectsMD.OverwriteDllfile = BoYesNoEnum.tYES;
                oUserObjectsMD.UseUniqueFormType = BoYesNoEnum.tNO;
                oUserObjectsMD.CanArchive = BoYesNoEnum.tNO;
                oUserObjectsMD.EnableEnhancedForm = BoYesNoEnum.tYES;
                oUserObjectsMD.RebuildEnhancedForm = objetoUsuario.RebuildEnhancedForm;
                //oUserObjectsMD.FormSRF = objetoUsuario.FormSRF;
                //oUserObjectsMD.ChildTables.TableName = objetoUsuario.TablaHijo;

                oUserObjectsMD.MenuItem = objetoUsuario.Menu_Item.Menu_Item;
                oUserObjectsMD.MenuUID = objetoUsuario.Menu_Item.MenuUID;
                oUserObjectsMD.MenuCaption = objetoUsuario.Menu_Item.MenuCaption;
                oUserObjectsMD.FatherMenuID = objetoUsuario.Menu_Item.FatherMenuID;
                oUserObjectsMD.Position = objetoUsuario.Menu_Item.Position;
                */

                oUserObjectsMD.Code = objetoUsuario.CodeObjeto;
                oUserObjectsMD.Name = objetoUsuario.NameObjeto;
                oUserObjectsMD.TableName = objetoUsuario.TablaPadre;
                oUserObjectsMD.LogTableName = objetoUsuario.TablaLog;
                oUserObjectsMD.ObjectType = objetoUsuario.Tipo;
                oUserObjectsMD.ManageSeries = objetoUsuario.ManageSeries;
                oUserObjectsMD.CanDelete = objetoUsuario.CanDelete;
                oUserObjectsMD.CanClose = objetoUsuario.CanClose;
                oUserObjectsMD.CanCancel = objetoUsuario.CanCancel;
                oUserObjectsMD.ExtensionName = string.Empty;
                oUserObjectsMD.CanFind = objetoUsuario.CanFind;
                oUserObjectsMD.CanYearTransfer = objetoUsuario.CanYearTransfer;
                oUserObjectsMD.CanLog = objetoUsuario.CanLog;
                oUserObjectsMD.OverwriteDllfile = BoYesNoEnum.tYES;
                oUserObjectsMD.UseUniqueFormType = BoYesNoEnum.tNO;
                oUserObjectsMD.CanArchive = BoYesNoEnum.tNO;

                oUserObjectsMD.EnableEnhancedForm = objetoUsuario.EnableEnhancedForm;
                oUserObjectsMD.RebuildEnhancedForm = objetoUsuario.RebuildEnhancedForm;
                oUserObjectsMD.FormSRF = objetoUsuario.FormSRF;
                oUserObjectsMD.CanCreateDefaultForm = objetoUsuario.CanCreateDefaultForm;

                oUserObjectsMD.MenuItem = objetoUsuario.Menu_Item.Menu_Item;
                oUserObjectsMD.MenuUID = objetoUsuario.Menu_Item.MenuUID;
                oUserObjectsMD.MenuCaption = objetoUsuario.Menu_Item.MenuCaption;
                oUserObjectsMD.FatherMenuID = objetoUsuario.Menu_Item.FatherMenuID;
                oUserObjectsMD.Position = objetoUsuario.Menu_Item.Position;

                oUserObjectsMD.ChildTables.TableName = objetoUsuario.TablaHijo;

                foreach (UDO2 FindCol in objetoUsuario.ListaUDO2)
                {
                    oUserObjectsMD.FindColumns.ColumnAlias = FindCol.ColAlias;
                    oUserObjectsMD.FindColumns.ColumnDescription = FindCol.ColDesc;

                    oUserObjectsMD.FindColumns.Add();
                }

                foreach (UDO3 FormCol in objetoUsuario.ListaUDO3)
                {
                    oUserObjectsMD.FormColumns.SonNumber = FormCol.SonNum;
                    oUserObjectsMD.FormColumns.FormColumnAlias = FormCol.ColAlias;
                    oUserObjectsMD.FormColumns.FormColumnDescription = FormCol.ColDesc;
                    oUserObjectsMD.FormColumns.Editable = FormCol.CanEdit;

                    oUserObjectsMD.FormColumns.Add();
                }

                foreach (UDO4 EnhFormCol in objetoUsuario.ListaUDO4)
                {
                    oUserObjectsMD.EnhancedFormColumns.ColumnNumber = EnhFormCol.ColNum;
                    oUserObjectsMD.EnhancedFormColumns.ChildNumber = EnhFormCol.SonNum;
                    oUserObjectsMD.EnhancedFormColumns.ColumnAlias = EnhFormCol.ColAlias;
                    oUserObjectsMD.EnhancedFormColumns.ColumnDescription = EnhFormCol.ColDesc;
                    oUserObjectsMD.EnhancedFormColumns.ColumnIsUsed = EnhFormCol.ColIsUsed;
                    oUserObjectsMD.EnhancedFormColumns.Editable = EnhFormCol.ColEdit;

                    oUserObjectsMD.EnhancedFormColumns.Add();
                }

                if (oUserObjectsMD.Add() != 0)
                {
                    err = ("(" + oCompany.GetLastErrorCode() + ") " + oCompany.GetLastErrorDescription());
                    logger.Error(err);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
                logger.Error("crearUDO[" + objetoUsuario.TablaPadre + "]: " + ex.Message, ex);
                return false;
            }
            finally
            {
                if (oUserObjectsMD != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserObjectsMD);
                oUserObjectsMD = null;
                GC.Collect();
            }
        }
    }
}