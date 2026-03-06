using System;
using System.Collections.Generic;
using SAPbobsCOM;

namespace ExxisBibliotecaClases
{
    public class BaseDatos
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(BaseDatos));
        private static Company oCompany;

        public BaseDatos(Company oCmpny)
        {
            oCompany = oCmpny;
        }

        public bool ValidarBD(string nombreBD)
        {
            Recordset oRecordSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            try
            {
                string query = "";
                if (oCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                {
                    try
                    {
                        query = "GRANT SELECT ON SCHEMA " + nombreBD + " to _SYS_REPO WITH GRANT OPTION";
                        oRecordSet.DoQuery(query);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    query = "select isnull(db_id('" + nombreBD + "'),'-1') as 'Existe'";
                    oRecordSet.DoQuery(query);
                    if (!oRecordSet.EoF)
                    {
                        string valor = oRecordSet.Fields.Item("Existe").Value.ToString();
                        return (valor != "-1");
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);
                oRecordSet = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            return false;
        }

        public string CrearBD(string nombreBD)
        {
            string error = string.Empty;
            Recordset oRecordSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            try
            {
                string query = "";
                if (oCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                {
                    query = "CREATE SCHEMA " + nombreBD;
                    oRecordSet.DoQuery(query);
                    query = "GRANT SELECT ON SCHEMA " + nombreBD + " to _SYS_REPO WITH GRANT OPTION";
                    oRecordSet.DoQuery(query);
                }
                else
                {
                    query = "CREATE DATABASE " + nombreBD;
                    oRecordSet.DoQuery(query);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                logger.Error(ex.Message, ex);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);
                oRecordSet = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            return error;
        }

        public bool CrearTabla(string nombreBD, string nombreTabla, Dictionary<string,string> camposTabla, out string error)
        {
            error = string.Empty;
            Recordset oRecordSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            try
            {
                string query = "";
                try
                {
                    query = "DROP PROCEDURE EXX_CREARTABLA_TEMP";
                    oRecordSet.DoQuery(query);
                }
                catch { }
                query = "CREATE PROCEDURE EXX_CREARTABLA_TEMP AS BEGIN ";
                if (oCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                {
                    query += "CREATE TABLE " + nombreBD + "." + nombreTabla + " ( ";
                }
                else
                {
                    query += "CREATE TABLE " + nombreBD + ".dbo." + nombreTabla + " (";
                }
                int numCampo = camposTabla.Count;
                foreach (KeyValuePair<string, string> campo in camposTabla)
                {
                    query += campo.Key + " " + campo.Value;
                    numCampo--;
                    if (numCampo > 0) query += ",";
                }
                query += "); END";
                oRecordSet.DoQuery(query);
                if (oCompany.DbServerType == BoDataServerTypes.dst_HANADB)
                {
                    query = "CALL EXX_CREARTABLA_TEMP";
                }
                else
                {
                    query = "EXEC EXX_CREARTABLA_TEMP";
                }
                oRecordSet.DoQuery(query);
                query = "DROP PROCEDURE EXX_CREARTABLA_TEMP";
                oRecordSet.DoQuery(query);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                logger.Error(ex.Message, ex);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);
                oRecordSet = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            return false;
        }
    }
}
