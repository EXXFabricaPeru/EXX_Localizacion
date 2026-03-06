using ExxisBibliotecaClases.entidades;
using ExxisBibliotecaClases.metodos;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExxisBibliotecaClases.query
{
    public class QueryExecutorMSSQL : QueryExecutorBase
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public QueryExecutorMSSQL(Company sBO_Company) : base(sBO_Company)
        {

        }
        protected override List<T> Execute<T>(string qryStr, bool resultados)
        {
            string[] queryLines;
            string queryAcum = "";

            queryLines = qryStr.Split('\n');

            List<T> lista = null;
            SAPbobsCOM.Recordset res = null;
            try
            {
                res = (Recordset)sBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                for (int i = 0; i < queryLines.Length; i++)
                {
                    String linea = queryLines[i].Trim();
                    linea = linea.StartsWith("--") ? "" : linea;
                    queryAcum += (linea == "GO" ? "" : " \n" + linea);
                    if (linea == "GO" || i == queryLines.Length - 1)
                    {
                        try
                        {
                            res.DoQuery(queryAcum);
                            logger.Debug($"Execute: {queryAcum}");
                            if (resultados)
                            {
                                lista = GetResultados<T>(res);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Execute: {queryAcum}", ex);
                            QueryExecutedEventArgs e = new QueryExecutedEventArgs()
                            {
                                Mensaje = ex.Message,
                                EsError = true
                            };
                            OnQueryExecuted(e);
                        }
                        queryAcum = "";
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                logger.Error($"Execute: {ex.Message}");
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(res);
            }
        } 
    }
}