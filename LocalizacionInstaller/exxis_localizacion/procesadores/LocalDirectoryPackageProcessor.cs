using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using exxis_localizacion.dataccess;
using exxis_localizacion.entidades;
using SAPbobsCOM;

namespace exxis_localizacion.procesadores
{
    public class LocalDirectoryPackageProcessor : BasePackageProcessor
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);
        
        private string _consolidadoPath;

        public LocalDirectoryPackageProcessor(SB1DataAccess pSB1DataAccess, string pConsolidadoPath):base(pSB1DataAccess)
        {
            _consolidadoPath = pConsolidadoPath;
        }

        public override void EjecutarProcesos(SB1Package pqt)
        {
            _progresoGeneral = 0;
            try
            {
                _maxContadorGeneral = 0;

                //ProcesarTablas();
                //ProcesarCampos();
                //ProcesarObjetos();
                //ProcesarCategoriasConsultasUsuario();
                //ProcesarConsultasUsuario();
                //ProcesarBusquedasFormateadas();
                //ProcesarDatosTablasUsuario();
                //ProcesarDatosObjetosUsuario();
                //ProcesarGEP($"{_consolidadoPath}\\GEP");

                _mensajeGeneral = "Finalizado!!!";
                EnviarProgreso(100,100, "", "Proceso finalizado", true);
            }
            catch (Exception ex)
            {
                _progresoGeneral = 0;
                _mensajeGeneral = "Proceso finalizado con errores";
                EnviarProgreso(0,100, "", ex.Message, true);
                throw ex;
            }
        }

        //private void ProcesarGEP(string geppath)
        //{
        //    logger.Info("ProcesarGEP: Inicio creación de creación de Formatos Electrónicos ");

        //    _mensajeGeneral = "Procesando Formatos Electrónicos (GEP)";

        //    String[] files = Directory.GetFiles(geppath, "*.GEP");

        //    int contador = 0;
        //    int maxContadorParcial = files.Count();

        //    if (files.Count() > 0)
        //    {
        //        foreach (String file in files)
        //        {
        //            String resultadoTexto = "";
        //            String mensaje = "Cargando " + file;
        //            logger.Debug($"ProcesarGEP: Cargando => {file}");

        //            if (!_SB1DataAccess.AddElectronicFormat(file, out String errorMsg))
        //            {
        //                resultadoTexto = errorMsg;
        //            }
        //            contador++;
        //            EnviarProgreso((contador * 100) / maxContadorParcial, mensaje, resultadoTexto, false);
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception("No hay datos para procesar");
        //    }

        //    logger.Info("ProcesarTablas: Fin creación de formato electrónicos ");

        //    _progresoGeneral += 1;
        //}
    }
}
