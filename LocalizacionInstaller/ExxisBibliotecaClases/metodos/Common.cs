using SAPbouiCOM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ExxisBibliotecaClases.metodos
{
    public class Common
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public static void LiberarObjeto(object objeto)
        {
            try
            {
                if (objeto != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(objeto);
                objeto = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch { }
        }
        public static void AgregarFiltroSeguro(SAPbouiCOM.Application sb1app,SAPbouiCOM.BoEventTypes tipoEvento,String formType) {
            //obtener filtro desde SAP B1 Application
            SAPbouiCOM.EventFilters oFilters = sb1app.GetFilter();
            if (oFilters == null)
            {
                oFilters = new SAPbouiCOM.EventFilters();
                sb1app.SetFilter(oFilters);
            }

            String nombreEvento = Enum.GetName(typeof(SAPbouiCOM.BoEventTypes), tipoEvento);

            //obtener filtro específico
            EventFilter oFilter = null;
            foreach (EventFilter ef in oFilters) {
                if (ef.EventType == tipoEvento) {
                    oFilter = ef;
                    break;
                }
            }
            if(oFilter == null)
            {
                oFilter = oFilters.Add(tipoEvento);
                logger.Debug($"AgregarFiltroSeguro: filtro agregado para evento '{nombreEvento}'");
            }

            FormType formTypeObj = null;
            foreach (FormType ft in oFilter)
            {
                if (ft.StringValue == formType)
                {
                    formTypeObj = ft;
                    break;
                }
            }

            if (formTypeObj != null)
            {
                logger.Warn($"AgregarFiltroSeguro: el formType '{formType}' para el evento '{nombreEvento}' ya existe!!!");
                return;
            }

            //agregar formulario
            oFilter.AddEx(formType);
            logger.Debug($"AgregarFiltroSeguro: se ha agregado el filtro para el formType '{formType}' para el evento '{nombreEvento}'");
        }
        public static void EliminarFiltroSeguro(SAPbouiCOM.Application sb1app, SAPbouiCOM.BoEventTypes tipoEvento, String formType)
        {
            //obtener filtro desde SAP B1 Application
            SAPbouiCOM.EventFilters oFilters = sb1app.GetFilter();
            if (oFilters == null)
            {
                logger.Warn("EliminarFiltroSeguro: no se ha definido un objeto de fitros");
                return;
            }

            String nombreEvento = Enum.GetName(typeof(SAPbouiCOM.BoEventTypes), tipoEvento);
            //obtener filtro específico
            EventFilter oFilter = null;
            foreach (EventFilter ef in oFilters)
            {
                if (ef.EventType == tipoEvento)
                {
                    oFilter = ef;
                    break;
                }
            }
            if (oFilter == null)
            {
                logger.Debug($"EliminarFiltroSeguro: no se ha definido un filtro para '{nombreEvento}'");
                return;
            }

            //eliminar formulario
            FormType formTypeObj = null;
            foreach (FormType ft in oFilter)
            {
                if (ft.StringValue == formType)
                {
                    formTypeObj = ft;
                    break;
                }
            }
            if (formTypeObj == null)
            {
                logger.Warn($"EliminarFiltroSeguro: no se ha encontrado el formType '{formType}' para el evento '{nombreEvento}'");
                return;
            }

            oFilter.RemoveEx(formType);
            logger.Debug($"EliminarFiltroSeguro: se ha eliminado el formType '{formType}' para el evento '{nombreEvento}'");            
        }
        public static bool CheckAddOnVersion(String companyBD)
        {
            String archivo = "version.info";
            String versionAsm = Assembly.GetEntryAssembly().GetName().Version.ToString();
            try
            {
                String companyVersion;
                if (!File.Exists(archivo))
                {
                    companyVersion = $"{companyBD},{versionAsm}";
                    File.WriteAllText(archivo, companyVersion);
                    logger.Debug($"CheckAddOnVersion: se ha detectado una nueva versión = {versionAsm}");
                    return false;
                }

                List<String> lineas = File.ReadAllLines(archivo).ToList();
                companyVersion = lineas.FirstOrDefault(x => x.Split(',')[0] == companyBD);
                if (companyVersion == null)
                {
                    companyVersion = $"{companyBD},{versionAsm}";
                    lineas.Add(companyVersion);
                    File.WriteAllLines(archivo, lineas);
                    logger.Debug($"CheckAddOnVersion: se ha detectado una nueva versión = {versionAsm}");
                    return false;
                }

                String versionLeido = companyVersion.Split(',')[1];
                if (versionLeido != versionAsm)
                {                    
                    int index = lineas.IndexOf(companyVersion);
                    companyVersion = $"{companyBD},{versionAsm}";
                    lineas[index] = companyVersion;
                    logger.Debug($"CheckAddOnVersion: se ha detectado una nueva versión {versionLeido} => {versionAsm}");
                    File.WriteAllLines(archivo, lineas);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"CheckAddOnVersion: {ex.Message}");
                throw ex;
            }
        }
    }    
}
