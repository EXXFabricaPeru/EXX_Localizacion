using ExxisBibliotecaClases.metodos;
using SAPbobsCOM;
using System;
using System.Reflection;
using System.Xml;

namespace ExxisBibliotecaClases
{
    public class UserObjectDataManager
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        private Company _company;

        public UserObjectDataManager(Company dicmp)
        {
            this._company = dicmp;
        }
        public String CheckFromXML(String udoCode,string xmlString, bool updateExisting)
        {
            UserObjectsMD oUDO = null;
            GeneralService oService = null;
            String msj = "";
            try
            {
                oService = _company.GetCompanyService().GetGeneralService(udoCode);
                GeneralData gdapp = (GeneralData)oService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                oUDO = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                if (!oUDO.GetByKey(udoCode))
                {
                    throw new Exception($"CheckFromXML: el objeto {udoCode} no ha sido encontrado");
                }
                String campollave = "";
                object valorllave;                

                try
                {                    
                    gdapp.FromXMLString(xmlString);
                    switch (oUDO.ObjectType)
                    {
                        case BoUDOObjType.boud_Document:
                            campollave = "DocEntry";
                            break;
                        case BoUDOObjType.boud_MasterData:
                            campollave = "Code";
                            break;
                    }

                    valorllave = getKeyFromXml(xmlString, campollave);
                }
                catch (Exception ex) {
                    throw new Exception($"{ex.Message} - xml : {xmlString}");
                }
                logger.Debug($"CheckFromXML: valor obtenidos... campollave = {campollave}, valorllave = {valorllave}");
                GeneralDataParams gdp = (GeneralDataParams)oService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                
                gdp.SetProperty(campollave, valorllave);
                logger.Debug($"CheckFromXML: valor de busqueda asignado... {campollave}={valorllave}");
                try
                {
                    GeneralData gdcmp = oService.GetByParams(gdp);
                    logger.Debug($"CheckFromXML: el regitro '{valorllave}' existe");
                    if (!updateExisting) {
                        msj = $"El registro '{valorllave}' no se actualizará";
                        logger.Info($"CheckFromXML: {msj}");
                        return msj;
                    }
                    if (!CompareObject(xmlString, gdapp, gdcmp))
                    {
                        try
                        {
                            updteObjectWithReference(oService, xmlString, gdapp, gdcmp);
                            msj = $"el registro '{valorllave}' ha sido actualizado";
                            logger.Info($"CheckFromXML: {msj}");
                        }
                        catch (Exception ex) {
                            throw new Exception($"Error al actualizar el registro '{valorllave}': {ex.Message}");
                        }
                    }
                    else
                    {
                        logger.Debug($"CheckFromXML: el registro '{valorllave}' es correcto");
                    }
                }
                catch {
                    //no existe. debe agregar.
                    try {
                        logger.Debug($"CheckFromXML: el registro '{valorllave}' no existe. Se intentará crear.");
                        oService.Add(gdapp);
                        msj = $"el registro '{valorllave}'  ha sido creado";
                        logger.Info($"CheckFromXML: {msj}");
                    }
                    catch(Exception ex)
                    {
                        throw new Exception($"Error al crear el registro '{valorllave}': {ex.Message}");
                    }                    
                }
            }
            catch (Exception ex)
            {
                msj = ex.Message;
                logger.Error($"CheckFromXML: {ex.Message}");
                throw ex;
            }
            finally {
                Common.LiberarObjeto(oUDO);
                Common.LiberarObjeto(oService);
            }
            return msj;
        }
        private void updteObjectWithReference(GeneralService oService,String xmlStringapp, GeneralData gdapp, GeneralData gdcmp)
        {
            //actualizando objeto obtenido desde la sociedad
            logger.Debug("updteObjectWithReference: asignando propiedades de cabecera de objeto");

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlStringapp);
                XmlNodeList campos = doc.DocumentElement.ChildNodes;
                foreach (XmlNode campo in campos)
                {
                    String nombreCampo = campo.Name;
                    if (nombreCampo == "DocEntry" || nombreCampo == "Code")
                    {
                        continue;
                    }
                    try
                    {
                        object valApp = gdapp.GetProperty(nombreCampo);
                        gdcmp.SetProperty(nombreCampo, valApp);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{nombreCampo} : {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"updteObjectWithReference: {ex.Message}");
            }

            oService.Update(gdcmp);
        }
        public void ClearData(string udoCode)
        {
            UserObjectsMD oUDO = null;
            GeneralService oService = null;
            Recordset rs = null;
            try
            {
                oService = _company.GetCompanyService().GetGeneralService(udoCode);
                GeneralData gdapp = (GeneralData)oService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                oUDO = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                if (!oUDO.GetByKey(udoCode))
                {
                    throw new Exception($"ClearData: el objeto {udoCode} no ha sido encontrado");
                }
                String campollave = "";                
                switch (oUDO.ObjectType)
                {
                    case BoUDOObjType.boud_Document:
                        campollave = "DocEntry";
                        break;
                    case BoUDOObjType.boud_MasterData:
                        campollave = "Code";
                        break;
                }

                String query = $"select \"{campollave}\" from \"@{oUDO.TableName}\"";
                try
                {                    
                    rs = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    rs.DoQuery(query);
                }
                catch (Exception ex)
                {
                    throw new Exception($"ClearData: {ex.Message} - query: {query}");
                }

                while (!rs.EoF)
                {
                    object valorllave = rs.Fields.Item(campollave).Value;
                    GeneralDataParams gdp = (GeneralDataParams)oService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                    gdp.SetProperty(campollave, valorllave);

                    try
                    {
                        oService.Delete(gdp);
                        logger.Info($"ClearData: el registro '{valorllave}' ha sido eliminado");
                    }
                    catch(Exception ex)
                    {                        
                        throw new Exception($"Error al eliminar el registro '{valorllave}': {ex.Message}");                        
                    }
                    rs.MoveNext();
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Common.LiberarObjeto(oUDO);
                Common.LiberarObjeto(oService);
                Common.LiberarObjeto(rs);
            }
        }
        private bool CompareObject(String xmlStringapp, GeneralData gdapp, GeneralData gdcmp)
        {
            logger.Debug($"CompareObject: validando registro");
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlStringapp);
                XmlNodeList campos = doc.DocumentElement.ChildNodes;
                foreach (XmlNode campo in campos)
                {
                    String nombreCampo = campo.Name;
                    try
                    {                        
                        object valApp = gdapp.GetProperty(nombreCampo);
                        object valCmp = gdcmp.GetProperty(nombreCampo);
                        if (!valApp.Equals(valCmp))
                        {
                            logger.Debug($"CompareObject: {nombreCampo} -> {valApp} != {valCmp}");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{nombreCampo} : {ex.Message}");
                    }
                }
            }
            catch (Exception ex) {
                logger.Error($"CompareObject: {ex.Message}");
                return false;
            }
           
            return true;
        }
        private object getKeyFromXml(string keyXml,String idllave)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(keyXml);
                XmlNode nLlave = doc.GetElementsByTagName(idllave).Item(0);
                if (idllave == "Code") {
                    return nLlave.InnerText;
                }
                int key = 0;
                Int32.TryParse(nLlave.InnerText,out key);
                if (key == 0) {
                    return new Exception($"'{nLlave.InnerText}' no es una llave válida");
                }
                return key;
            }
            catch (Exception ex)
            {
                logger.Error($"getKeyFromXml: {keyXml}", ex);
                throw ex;
            }
        }
    }
}