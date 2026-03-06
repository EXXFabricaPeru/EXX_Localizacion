using System;
using System.Reflection;
using ExxisBibliotecaClases.metodos;
using SAPbobsCOM;

namespace ExxisBibliotecaClases
{
    public class UserObjectManager:SB1EntityManager
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public UserObjectManager(Company dicmp):base(dicmp)
        {

        }

        public override string CheckFromXML(string xmlpath, int i, string Accion)
        {
            SAPbobsCOM.UserObjectsMD uoapp = null;
            SAPbobsCOM.UserObjectsMD uocmp = null;
            String msj = "";
            try
            {
                uoapp = (UserObjectsMD)_company.GetBusinessObjectFromXML(xmlpath, i);
                uocmp = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);

                if (uocmp.GetByKey(uoapp.Code))
                {
                    if (Accion == "R")
                    {
                        int ret = uocmp.Remove();
                        if (ret != 0)
                        {
                            throw new Exception($"{uoapp.Code}:({_company.GetLastErrorCode()}) - ({_company.GetLastErrorDescription()})");
                        }
                        msj = $"el objeto {uoapp.Code} ha sido eliminado";
                        logger.Info($"CheckFromXML: {msj}");
                    }
                    else
                    {
                        logger.Debug($"CheckFromXML: el objeto {uoapp.Code} existe");
                        if (!CompareObject(uoapp, uocmp))
                        {
                            int ret = updteObjectWithReference(uoapp, uocmp);
                            if (ret != 0)
                            {
                                throw new Exception($"{uoapp.Code}:({_company.GetLastErrorCode()}) - ({_company.GetLastErrorDescription()})");
                            }
                            msj = $"el objeto {uoapp.Code} ha sido actualizado";
                            logger.Info($"CheckFromXML: {msj}");
                        }
                        else
                        {
                            logger.Debug($"CheckFromXML: el objeto {uoapp.Code} es correcto");
                        }
                    }
                }
                else
                {
                    if (Accion != "R")
                    {
                        logger.Debug($"CheckFromXML: el objeto {uoapp.Code} no existe. Se intentará crear.");
                        int ret = uoapp.Add();
                        if (ret != 0)
                        {
                            throw new Exception($"{uoapp.Code}:({_company.GetLastErrorCode()}) - ({_company.GetLastErrorDescription()})");
                        }
                        msj = $"el objeto {uoapp.Code} ha sido creado";
                        logger.Info($"CheckFromXML: {msj}");
                    }
                    else
                        throw new Exception($" el UDO no existe, no se logró remover");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"CheckFromXML", ex);
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(uoapp);
                Common.LiberarObjeto(uocmp);
            }
            return msj;
        }
        private int updteObjectWithReference(UserObjectsMD uoapp, UserObjectsMD uocmp)
        {
            //actualizando objeto obtenido desde la sociedad
            logger.Debug("updteObjectWithReference: asignando propiedades de cabecera de objeto");
            try { uocmp.CanApprove = uoapp.CanApprove; } catch { }
            try { uocmp.CanArchive = uoapp.CanArchive; } catch { }
            try { uocmp.CanCancel = uoapp.CanArchive; } catch { }
            try { uocmp.CanClose = uoapp.CanClose; } catch { }
            try { uocmp.CanCreateDefaultForm = uoapp.CanCreateDefaultForm; } catch { }
            try { uocmp.CanDelete = uoapp.CanDelete; } catch { }
            try { uocmp.CanFind = uoapp.CanFind; } catch { }
            try { uocmp.CanLog = uoapp.CanLog; } catch { }
            try { uocmp.CanYearTransfer = uoapp.CanYearTransfer; } catch { }
            try { uocmp.EnableEnhancedForm = uoapp.EnableEnhancedForm; } catch { }
            try { uocmp.FatherMenuID = uoapp.FatherMenuID; } catch { }
            try { uocmp.FormSRF = uoapp.FormSRF; } catch { }
            try { uocmp.LogTableName = uoapp.LogTableName; } catch { }
            try { uocmp.ManageSeries = uoapp.ManageSeries; } catch { }
            try { uocmp.MenuCaption = uoapp.MenuCaption; } catch { }
            try { uocmp.MenuItem = uoapp.MenuItem; } catch { }
            try { uocmp.MenuUID = uoapp.MenuUID; } catch { }
            try { uocmp.Name = uoapp.Name; } catch { }
            try { uocmp.OverwriteDllfile = uoapp.OverwriteDllfile; } catch { }
            try { uocmp.Position = uoapp.Position; } catch { }
            try { uocmp.RebuildEnhancedForm = uoapp.RebuildEnhancedForm; } catch { }
            try { if (!String.IsNullOrEmpty(uoapp.TemplateID)) uocmp.TemplateID = uoapp.TemplateID; } catch { }
            try { uocmp.UseUniqueFormType = uoapp.UseUniqueFormType; } catch { }

            logger.Debug("updteObjectWithReference: asignando propiedades de tablas hijas");
            for (int i = 0; i < uoapp.ChildTables.Count; i++)
            {
                uoapp.ChildTables.SetCurrentLine(i);
                bool existe = false;
                for (int j = 0; j < uocmp.ChildTables.Count; j++)
                {
                    uocmp.ChildTables.SetCurrentLine(j);
                    if (uoapp.ChildTables.TableName == uocmp.ChildTables.TableName) {
                        existe = true;
                        break;
                    }
                }
                if (String.IsNullOrEmpty(uoapp.ChildTables.TableName))
                {
                    continue;
                }
                if (!existe)
                {
                    uocmp.ChildTables.Add();
                }
                uocmp.ChildTables.LogTableName = uoapp.ChildTables.LogTableName;
                uocmp.ChildTables.TableName = uoapp.ChildTables.TableName;
            }

            logger.Debug("updteObjectWithReference: asignando propiedades de columnas de formulario");
            for (int i = 0; i < uoapp.FormColumns.Count; i++)
            {
                uoapp.FormColumns.SetCurrentLine(i);
                bool existe = false;
                for (int j = 0; j < uocmp.FormColumns.Count; j++)
                {
                    uocmp.FormColumns.SetCurrentLine(j);
                    if (uoapp.FormColumns.FormColumnAlias == uocmp.FormColumns.FormColumnAlias)
                    {
                        existe = true;
                        break;
                    }
                }
                if (String.IsNullOrEmpty(uoapp.FormColumns.FormColumnAlias))
                {
                    continue;
                }
                if (!existe)
                {
                    uocmp.FormColumns.Add();
                }
                uocmp.FormColumns.Editable = uoapp.FormColumns.Editable;
                uocmp.FormColumns.FormColumnAlias = uoapp.FormColumns.FormColumnAlias;
                uocmp.FormColumns.FormColumnDescription = uoapp.FormColumns.FormColumnDescription;
                uocmp.FormColumns.SonNumber = uoapp.FormColumns.SonNumber;
            }

            logger.Debug("updteObjectWithReference: asignando propiedades de columnas de busqueda");
            for (int i = 0; i < uoapp.FindColumns.Count; i++)
            {
                uoapp.FindColumns.SetCurrentLine(i);
                bool existe = false;
                for (int j = 0; j < uocmp.FindColumns.Count; j++)
                {
                    uocmp.FindColumns.SetCurrentLine(j);
                    if (uoapp.FindColumns.ColumnAlias == uocmp.FindColumns.ColumnAlias)
                    {
                        existe = true;
                        break;
                    }
                }
                if (String.IsNullOrEmpty(uoapp.FindColumns.ColumnAlias))
                {
                    continue;
                }
                if (!existe)
                {
                    uocmp.FindColumns.Add();
                }
                uocmp.FindColumns.ColumnAlias = uoapp.FindColumns.ColumnAlias;
                uocmp.FindColumns.ColumnDescription = uoapp.FindColumns.ColumnDescription;
            }

            logger.Debug("updteObjectWithReference: asignando propiedades de columnas de formulario mejorado");
            for (int i = 0; i < uoapp.EnhancedFormColumns.Count; i++)
            {
                uoapp.EnhancedFormColumns.SetCurrentLine(i);
                bool existe = false;
                for (int j = 0; j < uocmp.EnhancedFormColumns.Count; j++)
                {
                    uocmp.EnhancedFormColumns.SetCurrentLine(j);
                    if (uoapp.EnhancedFormColumns.ColumnAlias == uocmp.EnhancedFormColumns.ColumnAlias)
                    {
                        existe = true;
                        break;
                    }
                }
                if (String.IsNullOrEmpty(uoapp.EnhancedFormColumns.ColumnAlias))
                {
                    continue;
                }
                if (!existe)
                {
                    uocmp.EnhancedFormColumns.Add();
                }
                uocmp.EnhancedFormColumns.ColumnAlias = uoapp.EnhancedFormColumns.ColumnAlias;
                uocmp.EnhancedFormColumns.ColumnDescription = uoapp.EnhancedFormColumns.ColumnDescription;
            }

            logger.Debug($"updteObjectWithReference: actualizando objeto {uocmp.Code}");
            return uocmp.Update();
        }

        private bool CompareObject(UserObjectsMD uoapp, UserObjectsMD uocmp)
        {
            logger.Debug($"CompareObject: validando objeto de usuario {uoapp.Code}");
            try
            {
                if (uoapp.Name != uocmp.Name)
                {
                    logger.Debug($"CompareObject: Name -> {uoapp.Name} != {uocmp.Name}");
                    return false;
                }

                if (uoapp.TableName != uocmp.TableName)
                {
                    logger.Debug($"CompareObject: TableName -> {uoapp.TableName} != {uocmp.TableName}");
                    return false;
                }

                if (uoapp.CanCreateDefaultForm != uocmp.CanCreateDefaultForm)
                {
                    logger.Debug($"CompareObject: CanCreateDefaultForm -> {uoapp.CanCreateDefaultForm} != {uocmp.CanCreateDefaultForm}");
                    return false;
                }

                if (uoapp.MenuItem != uocmp.MenuItem)
                {
                    logger.Debug($"CompareObject: MenuItem -> {uoapp.MenuItem} != {uocmp.MenuItem}");
                    return false;
                }

                if (uoapp.FormSRF != uocmp.FormSRF)
                {
                    logger.Debug($"CompareObject: FormSRF -> {uoapp.FormSRF} != {uocmp.FormSRF}");
                    return false;
                }

                if (uoapp.CanLog != uocmp.CanLog)
                {
                    logger.Debug($"CompareObject: CanLog -> {uoapp.CanLog} != {uocmp.CanLog}");
                    return false;
                }

                if (uoapp.EnableEnhancedForm != uocmp.EnableEnhancedForm)
                {
                    logger.Debug($"CompareObject: EnableEnhancedForm -> {uoapp.EnableEnhancedForm} != {uocmp.EnableEnhancedForm}");
                    return false;
                }

                if (uoapp.RebuildEnhancedForm != uocmp.RebuildEnhancedForm)
                {
                    logger.Debug($"CompareObject: RebuildEnhancedForm -> {uoapp.RebuildEnhancedForm} != {uocmp.RebuildEnhancedForm}");
                    return false;
                }

                if (!CompareObject_ChildTables(uoapp, uocmp))
                {
                    return false;
                }

                if (!CompareObject_FindColumns(uoapp, uocmp))
                {
                    return false;
                }

                if (!CompareObject_FormColumns(uoapp, uocmp))
                {
                    return false;
                }

                if (!CompareObject_EnhancedFormColumns(uoapp, uocmp))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("CompareObject",ex);
                return false;
            }

            logger.Debug($"CompareObject: son idénticos");

            return true;
        }
        private bool CompareObject_EnhancedFormColumns(UserObjectsMD uoapp, UserObjectsMD uocmp)
        {
            SAPbobsCOM.UserObjectMD_EnhancedFormColumns efccapp = uoapp.EnhancedFormColumns;
            SAPbobsCOM.UserObjectMD_EnhancedFormColumns efcccmp = uocmp.EnhancedFormColumns;

            for (int i = 0; i < efccapp.Count; i++)
            {
                efccapp.SetCurrentLine(i);
                bool fcexiste = false;
                for (int j = 0; j < efcccmp.Count; j++)
                {
                    efcccmp.SetCurrentLine(j);
                    if (efccapp.ColumnAlias == efcccmp.ColumnAlias)
                    {
                        fcexiste = true;
                        if (efccapp.ColumnDescription != efcccmp.ColumnDescription)
                        {
                            logger.Debug($"CompareObject_EnhancedFormColumns: {efccapp.ColumnAlias} - {efccapp.ColumnAlias} != {efcccmp.ColumnAlias}  - {efcccmp.ColumnAlias}");
                            return false;
                        }
                        break;
                    }
                }
                if (!fcexiste)
                {
                    logger.Debug($"CompareObject_EnhancedFormColumns: columna de formulario mejorado {efccapp.ColumnAlias} no existe");
                    return false;
                }
            }

            return true;
        }
        private bool CompareObject_FormColumns(UserObjectsMD uoapp, UserObjectsMD uocmp)
        {
            SAPbobsCOM.UserObjectMD_FormColumns formcapp = uoapp.FormColumns;
            SAPbobsCOM.UserObjectMD_FormColumns formccmp = uocmp.FormColumns;

            for (int i = 0; i < formcapp.Count; i++)
            {
                formcapp.SetCurrentLine(i);
                bool fcexiste = false;
                for (int j = 0; j < formccmp.Count; j++)
                {
                    formccmp.SetCurrentLine(j);
                    if (formcapp.FormColumnAlias == formccmp.FormColumnAlias)
                    {
                        fcexiste = true;
                        if (formcapp.FormColumnDescription != formccmp.FormColumnDescription)
                        {
                            logger.Debug($"CompareObject_FormColumns: {formcapp.FormColumnAlias} != {formccmp.FormColumnAlias}");
                            return false;
                        }
                        break;
                    }
                }
                if (!fcexiste)
                {
                    logger.Debug($"CompareObject_FormColumns: columna de formulario {formcapp.FormColumnAlias} no existe");
                    return false;
                }
            }

            return true;
        }
        private bool CompareObject_FindColumns(UserObjectsMD uoapp, UserObjectsMD uocmp)
        {
            SAPbobsCOM.UserObjectMD_FindColumns fcapp = uoapp.FindColumns;
            SAPbobsCOM.UserObjectMD_FindColumns fccmp = uocmp.FindColumns;

            for (int i = 0; i < fcapp.Count; i++)
            {
                fcapp.SetCurrentLine(i);
                bool fcexiste = false;
                for (int j = 0; j < fccmp.Count; j++)
                {
                    fccmp.SetCurrentLine(j);
                    if (fcapp.ColumnAlias == fccmp.ColumnAlias)
                    {
                        fcexiste = true;
                        if (fcapp.ColumnDescription != fccmp.ColumnDescription)
                        {
                            logger.Debug($"CompareObject_FindColumns: {fcapp.ColumnAlias} - {fcapp.ColumnDescription} != {fccmp.ColumnAlias}  - {fccmp.ColumnDescription}");
                            return false;
                        }
                        break;
                    }
                }
                if (!fcexiste)
                {
                    logger.Debug($"CompareObject_FindColumns: columna de busqueda {fcapp.ColumnAlias} no existe");
                    return false;
                }
            }

            return true;
        }
        private bool CompareObject_ChildTables(UserObjectsMD uoapp, UserObjectsMD uocmp)
        {
            logger.Debug($"CompareObject_ChildTables: validando tablas hijas de '{uoapp.Code}'");

            SAPbobsCOM.UserObjectMD_ChildTables ctapp = uocmp.ChildTables;
            SAPbobsCOM.UserObjectMD_ChildTables ctcmp = uoapp.ChildTables;

            for (int i = 0; i < ctapp.Count; i++)
            {
                ctapp.SetCurrentLine(i);
                bool ctexiste = false;
                for (int j = 0; j < ctcmp.Count; j++)
                {
                    ctcmp.SetCurrentLine(j);
                    if (ctapp.TableName == ctcmp.TableName)
                    {
                        ctexiste = true;
                        break;
                    }
                }
                if (!ctexiste)
                {
                    logger.Debug($"CompareObject_ChildTables: tabla hija '{ctapp.TableName}' no existe");
                    return false;
                }
            }

            return true;
        }
    }
}