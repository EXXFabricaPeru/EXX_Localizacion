using ExxisBibliotecaClases.entidades;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases
{
    public class MetadataManager
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public event EventHandler<VerificandoEstructuraEventArgs> VerificandoEstructura;

        private Company company;
        public UserTableMDManager UserTableMDManager { get; private set; }
        public UserFieldsManager UserFieldsManager { get; private set; }
        public UserObjectManager UserObjectManager { get; private set; }
        public UserObjectDataManager UserObjectDataManager { get; private set; }

        public const int UDT = 0x01;
        public const int UDF = 0x02;
        public const int UDO = 0x04;
        public const int UDOData = 0x08;
        public const int ALL = 0xFF;

        public MetadataManager(Company dicmp):this(dicmp, ALL)
        {

        }
        public MetadataManager(Company dicmp, int structureflags) {
            this.company = dicmp;
            if ((structureflags & UDT) == UDT)
            {
                this.UserTableMDManager = new UserTableMDManager(dicmp);
            }
            if ((structureflags & UDF) == UDF)
            { 
                this.UserFieldsManager = new UserFieldsManager(dicmp);
            }
            if ((structureflags & UDO) == UDO)
            { 
                this.UserObjectManager = new UserObjectManager(dicmp);
            }
            if ((structureflags & UDOData) == UDOData)
            { 
                this.UserObjectDataManager = new UserObjectDataManager(dicmp);
            }
        }
        public void CheckUserTables(String xmlpath)
        {
            try
            {
                if (UserTableMDManager == null)
                {
                    throw new Exception("No ha definido que este objeto para verificar Tablas de Usuario");
                }
                int count = company.GetXMLelementCount(xmlpath);
                logger.Info($"CheckUserTables: se han detectado {count} tabla de usuario");
                for (int i = 0; i < count; i++)
                {
                    UserTableMDManager.CheckFromXML(xmlpath, i, "A");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Verifica la existencia de los campos indicados. Si no existe la crea.
        /// Si el campo existe pero la definición es diferente entonces la actualiza.
        /// </summary>
        /// <param name="xmlpath">la ubicación en disco del archivo xml que contiene la definición de los campos de usuario</param>
        public void CheckUserFields(String xmlpath)
        {
            try
            {
                if (UserFieldsManager == null)
                {
                    throw new Exception("No ha definido que este objeto para verificar Campos de Usuario");
                }
                int count = company.GetXMLelementCount(xmlpath);
                logger.Info($"CheckUserFields: se han detectado {count} campos de usuario");
                for (int i = 0; i < count; i++)
                {
                    UserFieldsManager.CheckFromXML(xmlpath, i, "A");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Verifica la la definición de los campos para encontrar alguna ausencia o diferencia.
        /// </summary>
        /// <param name="xmlpath"></param>
        /// <returns>true si se encuentran diferencias en la estructura. falso si la estructura de campos es la misma</returns>
        public bool ValidateUserFields(String xmlpath, string Accion)
        {
            try
            {
                if (UserFieldsManager == null)
                {
                    throw new Exception("No ha definido que este objeto para verificar Campos de Usuario");
                }

                int count = company.GetXMLelementCount(xmlpath);
                logger.Info($"ValidateUserFields: se han detectado {count} campos de usuario");
                for (int i = 0; i < count; i++)
                {
                    UserFieldsManager.CheckFromXML(xmlpath, i,false, Accion);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"ValidateUserFields: {ex.Message}");
                return false;
            }
        }
        public void CheckUserObjects(String xmlpath, string Accion)
        {
            if (UserObjectManager == null)
            {
                throw new Exception("No ha definido que este objeto para verificar Objetos de Usuario");
            }

            int count = company.GetXMLelementCount(xmlpath);
            logger.Info($"CheckUserObjects: se han detectado {count} objetos de usuario");
            for (int i = 0; i < count; i++)
            {
                UserObjectManager.CheckFromXML(xmlpath, i, Accion);
            }
        }
        public void CheckUserObjectsData(string xmlpath,bool updateExisting)
        {
            try
            {
                if (UserObjectDataManager == null)
                {
                    throw new Exception("No ha definido que este objeto para verificar Datos de Objetos de Usuario");
                }

                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(xmlpath);
                String udoCode = doc.DocumentElement.Name;
                System.Xml.XmlNodeList instancias = doc.SelectSingleNode(udoCode).ChildNodes;
                logger.Info($"CheckUserObjectsData: se ha detectado el objeto {udoCode} con {instancias.Count} elementos");
                foreach (System.Xml.XmlNode nodo in instancias)
                {
                    UserObjectDataManager.CheckFromXML(udoCode,nodo.OuterXml, updateExisting);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Verifica la estructura de la ase de datos contra los archivos xml entregados
        /// </summary>
        /// <param name="paths">
        /// un arreglo de n posiciones donde cada elemento será precedido por las palabras
        /// UDT:,UDF:,UDO:,UDOData:, seguido de la ruta completa del archivo xml correspondiente
        /// </param>
        public void CheckStructure(string[] paths) {
            VerificandoEstructuraEventArgs args = new VerificandoEstructuraEventArgs();
            try
            {
                int _maxContador = 0;
                String[] rutas;
                Dictionary < int, List <Object[]>> info = new Dictionary<int, List<Object[]>>();
                if (UserTableMDManager != null)
                {
                    String pref = "UDT:";
                    rutas = paths.Where(x => x.StartsWith(pref)).ToArray();
                    List<Object[]> datos = new List<Object[]>();
                    foreach (String ruta in rutas)
                    {
                        String xmlpath = ruta.Substring(pref.Length);
                        int cont = company.GetXMLelementCount(xmlpath);
                        _maxContador += cont;
                        datos.Add(new object[] { xmlpath, cont });
                    }
                    info.Add(UDT,datos);
                }
                if (UserFieldsManager != null)
                {
                    String pref = "UDF:";
                    rutas = paths.Where(x => x.StartsWith(pref)).ToArray();
                    List<Object[]> datos = new List<Object[]>();
                    foreach (String ruta in rutas)
                    {
                        String xmlpath = ruta.Substring(pref.Length);
                        int cont = company.GetXMLelementCount(xmlpath);
                        _maxContador += cont;
                        datos.Add(new object[] { xmlpath, cont });
                    }
                    info.Add(UDF, datos);
                }
                if (UserObjectManager != null)
                {
                    String pref = "UDO:";
                    rutas = paths.Where(x => x.StartsWith(pref)).ToArray();
                    List<Object[]> datos = new List<Object[]>();
                    foreach (String ruta in rutas)
                    {
                        String xmlpath = ruta.Substring(pref.Length);
                        int cont = company.GetXMLelementCount(xmlpath);
                        _maxContador += cont;
                        datos.Add(new object[] { xmlpath, cont });
                    }
                    info.Add(UDO, datos);
                }
                if (UserObjectDataManager != null)
                {
                    String pref = "UDOData:";
                    rutas = paths.Where(x => x.StartsWith(pref)).ToArray();
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    List<Object[]> datos = new List<Object[]>();
                    foreach (String ruta in rutas)
                    {
                        String xmlpath = ruta.Substring(pref.Length);                        
                        doc.Load(xmlpath);
                        String udoCode = doc.DocumentElement.Name;
                        System.Xml.XmlNodeList instancias = doc.SelectSingleNode(udoCode).ChildNodes;
                        _maxContador += instancias.Count;
                        datos.Add(new object[] { xmlpath, instancias.Count});
                    }
                    info.Add(UDOData, datos);
                }
                int _progresoGeneral = 0;
                for (int i = 0; i < info.Count; i++)
                {
                    KeyValuePair<int,List<object[]>> objStruc = info.ElementAt(i);
                    foreach(object[] data in objStruc.Value)
                    {
                        String xmlpath = (String)data[0];
                        int cantidad = (int)data[1];
                        for (int j = 0; j < cantidad; j++)
                        {
                            try
                            {
                                switch (objStruc.Key)
                                {
                                    case UDT:
                                        args.Mensaje = new List<string>() { UserTableMDManager.CheckFromXML(xmlpath, j, "A") };
                                        break;
                                    case UDF:
                                        args.Mensaje = new List<string>() { UserFieldsManager.CheckFromXML(xmlpath, j, "A") };
                                        break;
                                    case UDO:
                                        args.Mensaje = new List<string>() { UserObjectManager.CheckFromXML(xmlpath, j, "A") };
                                        break;
                                    case UDOData:
                                        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                                        doc.Load(xmlpath);
                                        String udoCode = doc.DocumentElement.Name;
                                        System.Xml.XmlNode nodo = doc.SelectSingleNode(udoCode).ChildNodes[j];
                                        args.Mensaje = new List<string>() { UserObjectDataManager.CheckFromXML(udoCode, nodo.OuterXml, true) };
                                        break;
                                    default:
                                        throw new Exception($"Valor no sopotado: {objStruc.Key}");
                                }
                                _progresoGeneral++;                                
                                args.ProgresoGeneral = (int)(_progresoGeneral * 100.0) / _maxContador;
                            }
                            catch (Exception ex)
                            {
                                args.Mensaje = new List<string>() { ex.Message };
                                args.ExitenErrores = true;
                                throw ex;
                            }
                            finally
                            {
                                VerificandoEstructura?.Invoke(this, args);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
