using ExxisBibliotecaClases.entidades;
using ExxisBibliotecaClases.metodos;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExxisBibliotecaClases.query
{
    public abstract class QueryExecutorBase : IQueryExecutor
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        protected Company sBO_Company;

        public event EventHandler<QueryExecutedEventArgs> QueryExecuted;

        public QueryExecutorBase(Company sBO_Company)
        {
            this.sBO_Company = sBO_Company;
        }
        public void ExecuteUpdate(string updStr) {
            Execute(updStr,false);
        }
        public List<T> ExecuteQuery<T>(string qryStr){
            return Execute<T>(qryStr, true);
        }
        public T ExecuteSingleResult<T>(string qryStr)
        {
            List<T> lista = Execute<T>(qryStr, true);
            if (lista.Count > 0)
            {
                return lista.First();
            }
            else 
            {
                //logger.Warn("ExecuteSingleResult: El query no devolvió resultados");
                return default;
            }            
        }
        public List<RegistroGenerico> ExecuteQuery(string qryStr)
        {
            return Execute(qryStr, true);
        }
        protected List<RegistroGenerico> Execute(string qryStr, bool resultados)
        {
            return Execute<RegistroGenerico>(qryStr, resultados);
        }
        protected abstract List<T> Execute<T>(string qryStr, bool resultados);
        protected List<T> GetResultados<T>(Recordset res)
        {
            if (typeof(T) == typeof(RegistroGenerico))
            {
                return GetAsRegistroGenerico(res).Cast<T>().ToList();
            }
            else
            {
                return GetFromXML<T>(res);
            }
        }
        private List<RegistroGenerico> GetAsRegistroGenerico(Recordset res)
        {
            List<RegistroGenerico> lista = new List<RegistroGenerico>();

            try
            {
                if (res.EoF)
                {
                    logger.Warn("GetAsRegistroGenerico: No se han encontrado resultados en RecordSet");
                }

                while (!res.EoF)
                {
                    RegistroGenerico rg = new RegistroGenerico();
                    int colCount = res.Fields.Count;
                    for(int i = 0;i < colCount; i++) 
                    {
                        IField campo = res.Fields.Item(i);
                        rg.Campos.Add(campo.Name,campo.Value);
                    }
                    lista.Add(rg);
                    res.MoveNext();
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetAsRegistroGenerico", ex);
            }

            return lista;
        }
        protected List<T> GetFromXML<T>(Recordset res)
        {
            /*
             * Depende la clase entidades.Resultados
             */
            List<T> lista = new List<T>();

            try
            {
                if (res.EoF)
                {
                    logger.Debug("GetFromXML: No se han encontrado resultados en RecordSet");
                    return lista;
                }

                Type tipo = typeof(T);
                String entityName = getEntityName(tipo);
                String xml = res.GetAsXML();

                xml = xml.Replace("<row>", $"<{entityName}>");
                xml = xml.Replace("</row>", $"</{entityName}>");

                /*
                 * Sólo aplica para tipos primitivos o si es un tipo que no es primitvo pero que debe tratarse como uno.
                 */
                if(tipo.IsPrimitive || tipo == typeof(DateTime) || tipo == typeof(Decimal) || tipo == typeof(String))
                {
                    logger.Debug($"GetFromXML: T es primitivo => {entityName}");
                    // Se asume una única columna.
                    Field f = res.Fields.Item(0);
                    String nombreCol = f.Name;
                    xml = xml.Replace($"<{nombreCol}>", "");
                    xml = xml.Replace($"</{nombreCol}>", "");
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNode root = doc.DocumentElement;
                XmlNode nBO = root.SelectSingleNode("/BOM/BO");
                if (nBO == null)
                {
                    throw new Exception("No se han encontrado la ruta /BOM/BO");
                }
                XmlNode nResultados = nBO.LastChild;

                xml = nResultados.InnerXml;
                xml = $@"<?xml version=""1.0"" encoding=""utf-8"" ?><Resultados><Registros>{xml}</Registros></Resultados>";

                Resultados<T> resultados = Resultados<T>.GetFromXML(xml);
                lista = resultados.Registros.ToList();
            }
            catch (Exception ex)
            {
                logger.Error("GetFromXML", ex);
            }
            
            return lista;
        }
        private string getEntityName(Type tipo)
        {
            if (tipo == typeof(Int32))
            {
                return "int";
            }
            if (tipo == typeof(String))
            {
                return "string";
            }
            if (tipo == typeof(double))
            {
                return "double";
            }
            return tipo.Name;
        }
        protected virtual void OnQueryExecuted(QueryExecutedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<QueryExecutedEventArgs> handler = QueryExecuted;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
