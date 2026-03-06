using ExxisBibliotecaClases.entidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.query
{
    public class QueryFactory
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        private SAPbobsCOM.Company _sBO_Company;
        private string _queryName;
        private string _assemblyPath;
        readonly Dictionary<string, string> _parametros = new Dictionary<string, string>();
        private TipoFuenteQuery _tipo;
        private string _queryStr;
        public static string SetMSSQLSufijo { set; get; } = "_mssql";
        public static string SetHANASufijo { set; get; } = "_hana";
        public event EventHandler<QueryExecutedEventArgs> QueryExecuted;
        /// <summary>
        /// Devuelve un objeto QueryFactory que utiliza los recursos locales del proyeco y la conexión SB1 
        /// para determinar el comando.
        /// </summary>
        /// <param name="pSBO_Company">la instancia de compañia DI de SB1</param>
        /// <param name="queryName">nombre del query sin considerar sufijo de bd o extensión</param>
        /// <param name="assemblyPath">ruta de ensamblado para recursos incrustados</param>
        public QueryFactory(SAPbobsCOM.Company pSBO_Company, string queryName, string assemblyPath)
        {
            _sBO_Company = pSBO_Company;
            _queryName = queryName;
            _assemblyPath = assemblyPath;
            _tipo = TipoFuenteQuery.tfqAssemblyPath;
        }
        /// <summary>
        /// Devuelve un objeto QueryFactory que utiliza los recursos locales del proyeco y la conexión SB1 
        /// para determinar el comando.
        /// </summary>
        /// <param name="pSBO_Company">la instancia de compañia DI de SB1</param>
        /// <param name="queryStr">candena del comando compatible para HANA y MSSQL</param>
        public QueryFactory(SAPbobsCOM.Company pSBO_Company, String queryStr)
        {
            _sBO_Company = pSBO_Company;
            _queryStr = queryStr;
            _tipo = TipoFuenteQuery.tfqStr;
        }
        private IQueryExecutor GetQueryExecutor() {
            IQueryExecutor qe = null;
            switch (_sBO_Company.DbServerType) {
                case SAPbobsCOM.BoDataServerTypes.dst_HANADB:
                    qe = new QueryExecutorHANA(_sBO_Company);
                    break;
                default:
                    qe = new QueryExecutorMSSQL(_sBO_Company);
                    break;
            }
            qe.QueryExecuted += QueryExecuted;
            return qe;
        }
        public void ExecuteUpdate()
        {
            if (_tipo == TipoFuenteQuery.tfqAssemblyPath) 
            {
                String SPFileName = GetByDBType();
                _queryStr = GetFromAssembly(SPFileName);
            }
            String qryStr = ReplaceParams(_queryStr);
            IQueryExecutor queryExecutor = GetQueryExecutor();
            queryExecutor.ExecuteUpdate(qryStr);
        }
        public List<T> ExecuteQuery<T>()
        {
            if (_tipo == TipoFuenteQuery.tfqAssemblyPath)
            {
                String SPFileName = GetByDBType();
                _queryStr = GetFromAssembly(SPFileName);
            }
            String qryStr = ReplaceParams(_queryStr);
            IQueryExecutor queryExecutor = GetQueryExecutor();
            return queryExecutor.ExecuteQuery<T>(qryStr);
        }

        public T ExecuteSingleResult<T>()
        {
            if (_tipo == TipoFuenteQuery.tfqAssemblyPath)
            {
                String SPFileName = GetByDBType();
                _queryStr = GetFromAssembly(SPFileName);
            }
            String qryStr = ReplaceParams(_queryStr);
            IQueryExecutor queryExecutor = GetQueryExecutor();
            return queryExecutor.ExecuteSingleResult<T>(qryStr);
        }

        public List<RegistroGenerico> ExecuteQuery()
        {
            return ExecuteQuery<RegistroGenerico>();
        }
        public String GetQuery() {
            String SPFileName = GetByDBType();
            String qryStr = GetFromAssembly(SPFileName);
            qryStr = ReplaceParams(qryStr);
            logger.Debug($"GetQuery: ejecutando... {qryStr}");
            return qryStr;
        }
        public String ReplaceParams(String qryStr) {
            foreach (KeyValuePair<String, String> pareja in _parametros)
            {
                qryStr = qryStr.Replace($"${{{pareja.Key}}}", pareja.Value);
            }
            return qryStr;
        }
        private String GetFromAssembly(String sPFileName) {
            try
            {
                //obtiene el ensamblado principal.
                Assembly asm = Assembly.GetEntryAssembly();
                String qryResource = $"{asm.GetName().Name}.{_assemblyPath}.{sPFileName}.sql";
                logger.Debug($"GetFromAssembly: buscando = {qryResource}");
                String qryStr;
                using (System.IO.Stream s = asm.GetManifestResourceStream(qryResource))
                {
                    using (StreamReader sr = new System.IO.StreamReader(s))
                    {
                        qryStr = sr.ReadToEnd();
                        sr.Close();
                    }
                    s.Close();
                }
                return qryStr;
            }
            catch (Exception ex)
            {
                logger.Error($"GetFromAssembly: {ex.Message}");
                throw ex;
            }
        }
        private String GetByDBType() {
            switch (_sBO_Company.DbServerType) {
                case SAPbobsCOM.BoDataServerTypes.dst_HANADB:
                    return $"{_queryName}{SetHANASufijo}";
                default:
                    return $"{_queryName}{SetMSSQLSufijo}";
            }
        }
        public void SetInt(String paramName, Object paramVal)
        {
            String paramValFinal;

            paramValFinal = (paramVal == null ? "null" : $"{paramVal}");

            if (_parametros.ContainsKey(paramName))
            {
                _parametros[paramName] = paramValFinal;
            }
            else
            {
                _parametros.Add(paramName, paramValFinal);
            }
        }
        public void SetString(String paramName, Object paramVal) {
            String paramValFinal;

            paramValFinal = (paramVal == null ? "null" : $"'{paramVal}'");

            if (_parametros.ContainsKey(paramName)) {
                _parametros[paramName] = paramValFinal;
            }
            else {
                _parametros.Add(paramName, paramValFinal);
            }
        }
        public void SetDate(String paramName, Object paramVal)
        {
            String paramValFinal;

            paramValFinal = (paramVal == null ? "null" : $"'{Convert.ToDateTime(paramVal).ToString("yyyyMMdd")}'");

            if (_parametros.ContainsKey(paramName))
            {
                _parametros[paramName] = paramValFinal;
            }
            else
            {
                _parametros.Add(paramName, paramValFinal);
            }
        }        
    }
}
