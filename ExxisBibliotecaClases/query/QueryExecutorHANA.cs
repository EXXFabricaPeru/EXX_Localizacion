using ExxisBibliotecaClases.entidades;
using ExxisBibliotecaClases.metodos;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExxisBibliotecaClases.query
{
    public class QueryExecutorHANA : QueryExecutorBase
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public QueryExecutorHANA(Company sBO_Company) : base(sBO_Company)
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

                int begin = 0;
                for (int i = 0; i < queryLines.Length; i++)
                {
                    string linea = queryLines[i].Trim();
                    //linea = linea.StartsWith("--") ? "" : linea;
                    if (linea.ToUpper().StartsWith("CREATE PROCEDURE") || linea.ToUpper().StartsWith("CREATE FUNCTION")) begin += 1;
                    if (linea == "END;") begin -= 1;

                    //remplazar funciones de MSSQL en HANA
                    linea = linea.Replace("LEN(","LENGTH(");
                    queryAcum += " \n" + linea;
                    if (((linea.EndsWith(";") || linea == "END;") && begin == 0) || i == queryLines.Length - 1)
                    {
                        try
                        {
                            queryAcum = queryAcum.Trim();
                            if (!String.IsNullOrWhiteSpace(queryAcum))
                            {
                                res.DoQuery(queryAcum);
                                logger.Debug($"Execute: {queryAcum}");
                                if (resultados)
                                {
                                    lista = GetResultados<T>(res);
                                    break;
                                }
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