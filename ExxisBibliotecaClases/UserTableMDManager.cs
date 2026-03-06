using ExxisBibliotecaClases.metodos;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases
{
    public class UserTableMDManager : SB1EntityManager
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public UserTableMDManager(Company dicmp) : base(dicmp)
        {

        }
        /// <summary>
        /// Procesa la ruta de de archivo xml
        /// </summary>
        /// <param name="xmlpath">ruta del archivo xml</param>
        /// <param name="i"></param>
        /// <returns></returns>
        public override String CheckFromXML(string xmlpath, int i, string Accion)
        {
            SAPbobsCOM.UserTablesMD utapp = null;
            SAPbobsCOM.UserTablesMD utcmp = null;
            String msj = "";
            try
            {
                utapp = (UserTablesMD)_company.GetBusinessObjectFromXML(xmlpath, i);
                utcmp = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                if (utcmp.GetByKey(utapp.TableName))
                {
                    if (Accion == "R")
                    {
                        int ret = utcmp.Remove();
                        if (ret != 0)
                        {
                            throw new Exception($"{utapp.TableName}:({_company.GetLastErrorCode()}) - ({_company.GetLastErrorDescription()})");
                        }
                        msj = $"la tabla {utapp.TableName} ha sido eliminada";
                        logger.Info($"CheckFromXML: {msj}");
                    }
                    else
                    {
                        logger.Debug($"CheckFromXML: la tabla {utapp.TableName} existe");
                        if (!CompareTable(utapp, utcmp))
                        {
                            int ret = updateFromXml(utcmp, utapp);
                            if (ret != 0)
                            {
                                throw new Exception($"{utapp.TableName}:({_company.GetLastErrorCode()}) - ({_company.GetLastErrorDescription()})");
                            }
                            msj = $"la tabla {utapp.TableName} ha sido actualizada";
                            logger.Info($"CheckFromXML: {msj}");
                        }
                        else
                        {
                            logger.Debug($"CheckFromXML: la tabla {utapp.TableName} es correcta");
                        }
                    }
                }
                else
                {
                    if (Accion != "R")
                    {
                        logger.Debug($"CheckFromXML: la tabla {utapp.TableName} no existe. Se intentará crear.");
                        int ret = utapp.Add();
                        if (ret != 0)
                        {
                            throw new Exception($"{utapp.TableName}:({_company.GetLastErrorCode()}) - ({_company.GetLastErrorDescription()})");
                        }
                        msj = $"la tabla {utapp.TableName} ha sido creada";
                        logger.Info($"CheckFromXML: {msj}");
                    }
                    else
                        throw new Exception($" la tabla no existe, no se logró remover");
                }
            }
            catch (Exception ex)
            {
                msj = ex.Message;
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(utapp);
                Common.LiberarObjeto(utcmp);
            }
            return msj;
        }
        /// <summary>
        /// Procesa el string xml
        /// </summary>
        /// <param name="xmlStr">string xml</param>
        /// <param name="i"></param>
        /// <returns></returns>
        private bool CompareTable(UserTablesMD utapp1, UserTablesMD utapp2)
        {
            logger.Debug($"CompareTable: validando tabla de usuario {utapp1.TableName} ");
            if (utapp1.TableName != utapp2.TableName)
            {
                logger.Debug($"CompareTable: {utapp1.TableName} != {utapp2.TableName}");
                return false;
            }

            if (utapp1.TableDescription != utapp2.TableDescription)
            {
                logger.Debug($"CompareTable: {utapp1.TableDescription} != {utapp2.TableDescription}");
                return false;
            }

            if (utapp1.TableType != utapp2.TableType)
            {
                logger.Debug($"CompareTable: {utapp1.TableType} != {utapp2.TableType}");
                return false;
            }

            logger.Debug($"CompareTable: son idénticos");

            return true;
        }
        private int updateFromXml(UserTablesMD utcmp, UserTablesMD utapp)
        {
            int ret;
            try
            {
                utcmp.TableName = utapp.TableName;
                utcmp.TableDescription = utapp.TableDescription;
                utcmp.TableType = utapp.TableType;

                ret = utcmp.Update();
            }
            catch (Exception ex)
            {
                logger.Error("updateFromXml", ex);
                ret = -1;
            }

            return ret;
        }
    }
}
