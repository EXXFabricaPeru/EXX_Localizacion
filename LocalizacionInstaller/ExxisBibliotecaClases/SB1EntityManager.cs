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
    public abstract class SB1EntityManager
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        protected Company _company;

        public SB1EntityManager(Company pcompany)
        {
            _company = pcompany;
        }
        /// <summary>
        /// Procesa la ruta de de archivo xml
        /// </summary>
        /// <param name="xmlpath">ruta del archivo xml</param>
        /// <param name="i"></param>
        /// <returns></returns>
        public abstract String CheckFromXML(string xmlpath, int i, string Accion);
        /// <summary>
        /// Procesa el string xml
        /// </summary>
        /// <param name="xmlStr">string xml</param>
        /// <param name="i"></param>
        /// <returns></returns>
        public String CheckFromXMLString(string xmlStr, int i, string Accion)
        {
            string temp = "";
            try
            {
                temp = Path.GetTempFileName();
                File.WriteAllText(temp, xmlStr);
                string msj = CheckFromXML(temp, i, Accion);
                return msj;
            }
            catch (Exception ex)
            {
                logger.Error($"CheckFromXMLString", ex);
                throw ex;
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
        }
    }
}
