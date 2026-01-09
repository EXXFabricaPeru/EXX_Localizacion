using System;
using System.Reflection;
using ExxisBibliotecaClases.metodos;
using SAPbobsCOM;

namespace ExxisBibliotecaClases
{
    public class UserFieldsManager : SB1EntityManager
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public UserFieldsManager(Company company) : base(company)
        {

        }
        public String CheckFromXML(string xmlpath, int i, bool updatecreate, string Accion)
        {
            SAPbobsCOM.UserFieldsMD ufapp = null;
            SAPbobsCOM.UserFieldsMD ufcmp = null;
            String msj = "";
            try
            {
                ufapp = (UserFieldsMD)_company.GetBusinessObjectFromXML(xmlpath, i);
                ufcmp = (UserFieldsMD)_company.GetBusinessObject(BoObjectTypes.oUserFields);
                if (Buscar(ufapp.TableName, ufapp.Name, ref ufcmp))
                {
                    if (Accion == "R")
                    {
                        int ret = ufcmp.Remove();
                        if (ret != 0)
                        {
                            throw new Exception($"{ufapp.TableName}.{ufapp.Name}:({_company.GetLastErrorCode()}) {_company.GetLastErrorDescription()}");
                        }
                        msj = $"el campo {ufapp.TableName}.{ufapp.Name} ha sido eliminado";
                        logger.Info($"CheckFromXML: {msj}");
                    }
                    else
                    {
                        logger.Debug($"CheckFromXML: el campo {ufapp.TableName}.{ufapp.Name} existe");
                        if (!CompareField(ufapp, ufcmp))
                        {
                            if (!updatecreate)
                            {
                                throw new Exception($"El campo {ufapp.TableName}.{ufapp.Name} es diferente");
                            }
                            int ret = updateFromXml(ufcmp, ufapp);
                            if (ret != 0)
                            {
                                throw new Exception($"{ufapp.TableName}.{ufapp.Name}:({_company.GetLastErrorCode()}) {_company.GetLastErrorDescription()}");
                            }
                            msj = $"el campo {ufapp.TableName}.{ufapp.Name} ha sido actualizado";
                            logger.Info($"CheckFromXML: {msj}");
                        }
                        else
                        {
                            logger.Debug($"CheckFromXML: el campo {ufapp.TableName}.{ufapp.Name} es correcto");
                        }
                    }
                }
                else
                {
                    if (Accion != "R")
                    {
                        if (!updatecreate)
                        {
                            throw new Exception($"El campo {ufapp.TableName}.{ufapp.Name} no existe");
                        }
                        else
                        {
                            logger.Debug($"CheckFromXML: El campo {ufapp.TableName}.{ufapp.Name} no existe.");
                        }
                        logger.Debug($"CheckFromXML: Se intentará crear.");
                        int ret = ufapp.Add();
                        if (ret != 0)
                        {
                            throw new Exception($"{ufapp.TableName}.{ufapp.Name}:({_company.GetLastErrorCode()}) {_company.GetLastErrorDescription()}");
                        }
                        msj = $"el campo {ufapp.TableName}.{ufapp.Name} ha sido creado";
                        logger.Info($"CheckFromXML: {msj}");
                    }
                    else
                        throw new Exception($" el campo no existe, no se logró remover");
                }
            }
            catch (Exception ex)
            {
                msj = ex.Message;
                logger.Error($"CheckFromXML: {ex.Message}");
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(ufapp);
                Common.LiberarObjeto(ufcmp);
            }
            return msj;
        }
        public override String CheckFromXML(string xmlpath, int i, string Accion)
        {
            return CheckFromXML(xmlpath, i, true, Accion);
        }
        private int updateFromXml(UserFieldsMD ufcmp, UserFieldsMD ufapp)
        {
            int ret;
            try
            {
                ufcmp.Description = ufapp.Description;
                ufcmp.EditSize = ufapp.EditSize;
                ufcmp.Mandatory = ufapp.Mandatory;
                ufcmp.Type = ufapp.Type;
                ufcmp.SubType = ufapp.SubType;
                if (ufcmp.Mandatory == BoYesNoEnum.tYES)
                {
                    ufcmp.DefaultValue = ufapp.DefaultValue;
                }
                SAPbobsCOM.ValidValuesMD vvcmp = ufcmp.ValidValues;
                while (vvcmp.Count > 0)
                {
                    vvcmp.SetCurrentLine(0);
                    vvcmp.Delete();
                }
                SAPbobsCOM.ValidValuesMD vvapp = ufapp.ValidValues;
                for (int i = 0; i < vvapp.Count; i++)
                {
                    vvapp.SetCurrentLine(i);
                    if (!String.IsNullOrEmpty(vvapp.Value) && !String.IsNullOrEmpty(vvapp.Description))
                    {
                        vvcmp.Value = vvapp.Value;
                        vvcmp.Description = vvapp.Description;
                        vvcmp.Add();
                    }
                }

                ret = ufcmp.Update();
            }
            catch (Exception ex)
            {
                logger.Error("updateFromXml", ex);
                ret = -1;
            }

            return ret;
        }
        private bool CompareField(UserFieldsMD ufapp, UserFieldsMD ufcmp)
        {
            logger.Debug($"CompareField: validando campo de usuario {ufapp.TableName}.{ufapp.Name} ");

            if (ufapp.Description != ufcmp.Description)
            {
                logger.Debug($"CompareField: {ufapp.Description} != {ufcmp.Description}");
                return false;
            }

            if (ufapp.EditSize != ufcmp.EditSize)
            {
                logger.Debug($"CompareField: {ufapp.EditSize} != {ufcmp.EditSize}");
                return false;
            }

            if (ufapp.Mandatory != ufcmp.Mandatory)
            {
                logger.Debug($"CompareField: {ufapp.Mandatory} != {ufcmp.Mandatory}");
                return false;
            }

            if (ufapp.Mandatory == BoYesNoEnum.tYES && ufapp.DefaultValue != ufcmp.DefaultValue)
            {
                logger.Debug($"CompareField: {ufapp.DefaultValue} != {ufcmp.DefaultValue}");
                return false;
            }

            if (ufapp.Type != ufcmp.Type)
            {
                logger.Debug($"CompareField: {ufapp.Type} != {ufcmp.Type}");
                return false;
            }

            if (ufapp.SubType != ufcmp.SubType)
            {
                logger.Debug($"CompareField: {ufapp.SubType} != {ufcmp.SubType}");
                return false;
            }

            SAPbobsCOM.ValidValuesMD vvcmp = ufcmp.ValidValues;
            SAPbobsCOM.ValidValuesMD vvapp = ufapp.ValidValues;

            for (int i = 0; i < vvapp.Count; i++)
            {
                vvapp.SetCurrentLine(i);
                bool vvexiste = false;
                for (int j = 0; j < vvcmp.Count; j++)
                {
                    vvcmp.SetCurrentLine(j);
                    if (vvapp.Value == vvcmp.Value)
                    {
                        vvexiste = true;
                        if (vvapp.Description != vvcmp.Description)
                        {
                            logger.Debug($"CompareField: {vvapp.Value} - {vvapp.Description} != {vvcmp.Value}  - {vvcmp.Description}");
                            return false;
                        }
                        break;
                    }
                }
                if (!vvexiste)
                {
                    logger.Debug($"CompareField: valor válido {vvapp.Value} no existe");
                    return false;
                }
            }

            logger.Debug($"CompareField: son idénticos");

            return true;
        }
        private bool Buscar(string tableName, string name, ref SAPbobsCOM.UserFieldsMD ufcmp)
        {
            logger.Debug($"Buscar: buscando {tableName}.{name} ...");
            SAPbobsCOM.Recordset rs = null;
            try
            {
                rs = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery($"Select \"FieldID\" FROM CUFD WHERE \"TableID\"='{tableName}' AND \"AliasID\"='{name}'");
                if (!rs.EoF)
                {
                    int fid = Convert.ToInt32(rs.Fields.Item("FieldID").Value);
                    logger.Debug($"Buscar: se encontró {fid}");
                    return ufcmp.GetByKey(tableName, fid);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Buscar", ex);
                return false;
            }
            finally
            {
                if (rs != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
                rs = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
    }
}