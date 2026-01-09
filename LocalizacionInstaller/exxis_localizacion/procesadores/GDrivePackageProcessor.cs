using exxis_localizacion.dataccess;
using exxis_localizacion.datasource;
using exxis_localizacion.entidades;
using ExxisBibliotecaClases.entidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace exxis_localizacion.procesadores
{
    public class GDrivePackageProcessor : BasePackageProcessor
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        private SB1Package paquete;

        private const int MaxLineLength = 110;

        public GDrivePackageProcessor(SB1DataAccess pSB1DataAccess) : base(pSB1DataAccess)
        {

        }

        public override void EjecutarProcesos(SB1Package pqt)
        {
            paquete = pqt;
            _progresoGeneral = 0;
            try
            {
                logger.Info($"EjecutarProcesos: Procesando paquete '{paquete.Nombre}' ({paquete.TipoBD})");
                bool eshana = paquete.TipoBD.CompareTo("HANA") == 0;
                _maxContadorGeneral = 11 + (eshana ? 1 : 0);
                EnviarProgreso(0, 100, "", DecorarEtiqueta(paquete.Nombre.ToUpper(), 1), false);
                ProcesarTablas();
                ProcesarCampos();
                ProcesarObjetos();
                ProcesarDatosTablasUsuario();
                ProcesarDatosObjetosUsuario();
                ProcesarCategoriasConsultasUsuario();
                ProcesarConsultasUsuario();
                ProcesarBusquedasFormateadas();
                if (eshana) ProcesarVistas();
                ProcesarScriptsNew();
                //ProcesarScripts();
                ProcesarGEP();
                ProcesarCrystal();

                EnviarProgreso(100, 100, "", "\n", false);
            }
            catch (Exception ex)
            {
                _progresoGeneral = 0;
                _mensajeGeneral = "Proceso finalizado con errores";
                EnviarProgreso(0, 100, "", ex.Message, false);
                throw ex;
            }
        }

        private void ProcesarTablas()
        {
            logger.Info("ProcesarTablas: Inicio creación de tablas de usuario ");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Tablas de usuario";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("TABLAS DE USUARIO", 2), false);

            List<TablaUsuarioWrp> registros = paquete.ListaTablas;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = registros.Count();

            if (registros != null && registros.Count > 0)
            {
                foreach (TablaUsuarioWrp utb in registros)
                {
                    String resultadoTexto = "";
                    String mensaje = $"Agregando tabla de usuario '{utb.O.Nombre}'...";
                    logger.Info($"ProcesarTablas: {mensaje}");
                    if (!_SB1DataAccess.AddUserTable(utb.O, out String errorMsg, utb.Accion))
                    {
                        resultadoTexto = $"[ERROR] [DRIVE Linea {contador + 3}] " + errorMsg;
                    }
                    else contadorOK++;
                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, resultadoTexto, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} TABLAS DE USUARIO procesados correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarTablas: Fin creación de tablas de usuario ");

            _progresoGeneral += 1;
        }
        private void ProcesarCampos()
        {
            logger.Info("ProcesarCampos: Inicio creación de campos de usuario");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Campos de usuario";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("CAMPOS DE USUARIO", 2), false);

            List<CampoUsuarioWrp> cabecera = paquete.ListaCampos;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = cabecera.Count();

            if (cabecera != null && cabecera.Count > 0)
            {
                foreach (CampoUsuarioWrp ufd in cabecera)
                {
                    String resultadoTexto = "";
                    String mensaje = $"Agregando campo de usuario {ufd.O.Tabla}.{ufd.O.Codigo}...";
                    logger.Info($"ProcesarCampos: {mensaje}");
                    if (!ProcesarCampo(ufd.O, out String errorMsg, ufd.Accion))
                    {
                        resultadoTexto = errorMsg;
                    }
                    else contadorOK++;
                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, resultadoTexto, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} CAMPOS DE USUARIO procesados correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarCampos: Fin creación de campos de usuario ");

            _progresoGeneral += 1;
        }
        private bool ProcesarCampo(CampoUsuario ufdo, out string errorMsg, string Accion)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(ufdo.LinkedUDO))
                {
                    string linkedudo = ufdo.LinkedUDO;
                    if (!_SB1DataAccess.UserObjectExists(linkedudo))
                    {
                        logger.Warn($"ProcesarCampos: el objeto vinculado {linkedudo} no existe. Se intenta crear con su estructura completa");

                        ObjetoUsuario objVinc = paquete.ListaObjetos.FirstOrDefault(x => x.O.CodeObjeto == linkedudo).O;
                        if (objVinc == null)
                        {
                            errorMsg = $"[ERROR] Campo \"{ufdo.Tabla}\".\"{ufdo.Codigo}\" => el objeto vinculado \"{linkedudo}\" no existe en la base de datos y tampoco en el repositorio cargado";
                            return false;
                        }
                        //No se intentará crear las tablas vinculadas al UDO 
                        //porque se asume que ya fueron creadas en el proceso previo.

                        //creando campos vinculados.
                        //listando tablas vinculadas.
                        List<string> tablas_vinculadas = objVinc.ChildTables.Select(x => $"@{x.TableName}").ToList();
                        //agregando tabla cabecera a lista.
                        tablas_vinculadas.Add($"@{objVinc.TablaPadre}");
                        List<CampoUsuarioWrp> camposvinc = paquete.ListaCampos.Where(x => tablas_vinculadas.Contains(x.O.Tabla)).ToList();
                        logger.Debug($"ProcesarCampo: se han encontrado {camposvinc.Count} campos de usuario vinculados para el objeto {linkedudo}");
                        foreach (CampoUsuarioWrp cu in camposvinc)
                        {
                            //basta con que un campo vinculado no pueda ser creado entonces el proceso para este campo se detiene
                            if (!ProcesarCampo(cu.O, out errorMsg, Accion))
                            {
                                errorMsg = $"[Error] Campo relacionado {errorMsg}";
                                return false;
                            }
                        }
                        //todos los campos ha sido creados entonces se procede a crear el objeto
                        bool udocreado = _SB1DataAccess.AddUserObject(objVinc, out errorMsg, Accion);
                        if (!udocreado)
                        {
                            errorMsg = $"[Error] Objeto relacionado {errorMsg}";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = $"[Error] Objeto relacionado: {ex.Message}";
                logger.Error("ProcesarCampo", ex);
                return false;
            }
            return _SB1DataAccess.AddUserField(ufdo, out errorMsg, Accion);
        }
        private void ProcesarObjetos()
        {
            logger.Info("ProcesarObjetos: Inicio creación de objetos de usuario");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Objetos de usuario";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("OBJETOS DE USUARIO", 2), false);

            List<ObjetoUsuarioWrp> cabecera = paquete.ListaObjetos;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = cabecera.Count();

            if (cabecera != null && cabecera.Count > 0)
            {
                foreach (ObjetoUsuarioWrp udo in cabecera)
                {
                    String resultadoTexto = "";
                    String mensaje = $"Agregando objeto de usuario {udo.O.CodeObjeto}...";
                    logger.Info($"ProcesarObjetos: {mensaje}");
                    if (!_SB1DataAccess.AddUserObject(udo.O, out string errorMsg, udo.Accion))
                    {
                        resultadoTexto = errorMsg;
                    }
                    else contadorOK++;
                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, resultadoTexto, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} OBJETOS DE USUARIO procesados correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarObjetos: Fin creación de objetos de usuario ");

            _progresoGeneral += 1;
        }
        private void ProcesarDatosTablasUsuario()
        {
            logger.Info("ProcesarDatosTablasUsuario: Inicio de poblado de tablas de usuario");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Datos de tablas de usuario";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("DATOS DE TABLAS DE USUARIO", 2), false);

            List<DatosTabla> datos = paquete.ListaDatosTablas;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = datos.Count;

            if (datos.Count > 0)
            {
                //foreach (DatosTabla tabla in datos)
                //{
                //    IList<IList<object>> datostabla = (IList<IList<object>>)tabla.Value;

                //    if (datostabla == null || datostabla.Count == 0)
                //    {
                //        throw new Exception("No hay datos para procesar");
                //    }

                //    String mensaje = $"Agregando datos de tablas de usuario \"{tabla.Key}\" ...";
                //    logger.Info($"ProcesarDatosTablasUsuario: {mensaje}");
                //    EnviarProgreso(contador, maxContadorParcial, mensaje, "", false);
                //    List<string> errorMsgs = new List<string>();
                //    if (!_SB1DataAccess.AddUserTableData(tabla.Key, (IList<IList<object>>)tabla.Value, ref errorMsgs))
                //    {
                //        foreach (string errMsg in errorMsgs)
                //        {
                //            EnviarProgreso(contador, maxContadorParcial, mensaje, errMsg, false);
                //        }
                //    }
                //    contador++;
                //    EnviarProgreso(contador, maxContadorParcial, mensaje, "", false);
                //}
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} DATOS DE TABLAS DE USUARIO procesados correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarDatosTablasUsuario: Fin poblado de tablas de usuario ");
            _progresoGeneral += 1;
        }
        private void ProcesarDatosObjetosUsuario()
        {
            logger.Info("ProcesarDatosObjetosUsuario: Inicio de poblado de objetos de usuario");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Datos de objetos de usuario";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("DATOS DE OBJETOS DE USUARIO", 2), false);

            List<DatosObjetoWrp> datos = paquete.ListaDatosObjetos;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = datos.Count;

            if (datos.Count > 0)
            {
                foreach (DatosObjetoWrp datosobjeto in datos)
                {
                    if (datosobjeto.O.Registros.Count == 0)
                    {
                        throw new Exception("No hay datos para procesar");
                    }

                    String mensaje = $"Agregando datos de objetos de usuario \"{datosobjeto.O.Codigo}\" ...";
                    logger.Info($"ProcesarDatosObjetosUsuario: {mensaje}");
                    EnviarProgreso(contador, maxContadorParcial, mensaje, "", false);
                    List<string> errorMsgs = new List<string>();
                    if (!_SB1DataAccess.AddUserObjectData(datosobjeto.O, ref errorMsgs))
                    {
                        foreach (string errMsg in errorMsgs)
                        {
                            EnviarProgreso(contador, maxContadorParcial, mensaje, errMsg, false);
                        }
                    }
                    else contadorOK++;
                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, "", false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} DATOS DE OBJETOS DE USUARIO procesados correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarDatosObjetosUsuario: Fin poblado de objetos de usuario ");

            _progresoGeneral += 1;
        }
        private void ProcesarCategoriasConsultasUsuario()
        {
            logger.Info("ProcesarCategoriasConsultasUsuario: Inicio creación de Categorías de Consultas de Usuario ");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Categorías de Consultas de Usuario";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("CATEGORÍAS DE CONSULTAS", 2), false);

            List<CategoriaConsultaWrp> datos = paquete.ListaCategorias;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = datos.Count();

            if (datos != null && datos.Count > 0)
            {
                foreach (CategoriaConsultaWrp cat1 in datos)
                {
                    CategoriaConsulta cat = cat1.O;
                    String resultadoTexto = "";
                    String mensaje = $"Agregando categoría de consulta de usuario '{cat.Name}'...";
                    logger.Info($"ProcesarCategoriasConsultasUsuario: {mensaje}");
                    if (!_SB1DataAccess.AddUserQueryCategory(ref cat, out String errorMsg))
                    {
                        resultadoTexto = errorMsg;
                    }
                    else contadorOK++;
                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, resultadoTexto, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} CATEGORÍAS DE CONSULTAS procesadas correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarCategoriasConsultasUsuario: Fin creación de Categorías de Consultas de Usuario ");

            _progresoGeneral += 1;
        }
        private void ProcesarConsultasUsuario()
        {
            logger.Info("ProcesarConsultasUsuario: Inicio creación de campos de usuario");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Consultas de usuario";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("CONSULTAS DE USUARIO", 2), false);

            List<ConsultaUsuarioWrp> userqueries = paquete.ListaConsultasUsuario;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = userqueries.Count();

            if (userqueries != null && userqueries.Count > 0)
            {
                foreach (ConsultaUsuarioWrp uqry1 in userqueries)
                {
                    ConsultaUsuario uqry = uqry1.O;
                    String resultadoTexto = "";
                    String mensaje = $"Agregando consulta de usuario \"{uqry.CategoriaConsultaRef.Name}\".\"{uqry.QueryDescription}\"...";
                    logger.Info($"ProcesarConsultasUsuario: {mensaje}");
                    uqry.Query = ReemplazarPaqueteHANA(uqry.Query);
                    if (!_SB1DataAccess.AddUserQuery(ref uqry, out String errorMsg, uqry1.Accion))
                    {
                        resultadoTexto = errorMsg;
                    }
                    else contadorOK++;
                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, resultadoTexto, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} CONSULTAS DE USUARIO procesadas correctamente", 3, false), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3), false);
            }

            logger.Info("ProcesarConsultasUsuario: Fin creación de consulta de usuario ");

            _progresoGeneral += 1;
        }
        private void ProcesarBusquedasFormateadas()
        {
            logger.Info("ProcesarBusquedasFormateadas: Inicio creación de busquedas formateadas");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Busquedas formateadas";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("BUSQUEDAS FORMATEADAS", 2), false);

            List<BusquedaFormateadaWrp> formattedsearches = paquete.ListaBusquedasFormateadas;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = formattedsearches.Count();

            if (formattedsearches != null && formattedsearches.Count > 0)
            {
                foreach (BusquedaFormateadaWrp formattedsearch1 in formattedsearches)
                {
                    BusquedaFormateada bf = formattedsearch1.O;
                    String resultadoTexto = "";
                    String mensaje = $"Agregando busqueda formateada \"{bf.FormID}\".\"{bf.ItemID}\".\"{bf.ColumnID}\" ...";
                    logger.Info($"ProcesarBusquedasFormateadas: {mensaje}");
                    if (!_SB1DataAccess.AddFormattedSearch(ref bf, out String errorMsg))
                    {
                        resultadoTexto = errorMsg;
                    }
                    else contadorOK++;
                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, resultadoTexto, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} BUSQUEDAS FORMATEADAS procesadas correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarBusquedasFormateadas: Fin creación de busquedas formateadas ");

            _progresoGeneral += 1;
        }
        private void ProcesarVistas()
        {
            logger.Info("ProcesarVistas: Inicio importación de vistas");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Vistas";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("VISTAS", 2), false);

            string modelovistapath = paquete.ModeloVistaPath;

            if (!string.IsNullOrWhiteSpace(modelovistapath))
            {
                EnviarProgreso(0, 100, "Importando vistas", "", false, modelovistapath);
                EnviarProgreso(100, 100, "", DecorarEtiqueta($"VISTAS procesadas", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarVistas: Fin creación de importación de VISTAS");

            _progresoGeneral += 1;
        }
        private void ProcesarScriptsNew()
        {
            logger.Info("ProcesarScripts: Inicio creación de Scripts");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Scripts";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("SCRIPTS", 2), false);

            List<Script> userqueries = paquete.ListaScriptsNew;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = userqueries.Count();

            if (userqueries != null && userqueries.Count() > 0)
            {
                foreach (Script script in userqueries)
                {
                    ConsultaUsuario uqry = script.O;
                    String resultadoTexto = "";
                    String mensaje = $"Agregando script \"{uqry.QueryDescription}\"...";
                    logger.Info($"ProcesarScripts: {mensaje}");
                    uqry.Query = ReemplazarPaqueteHANA(uqry.Query);
                    if (!_SB1DataAccess.AddScriptNew(script, out String errorMsg))
                    {
                        resultadoTexto = errorMsg;
                    }
                    else contadorOK++;
                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, errorMsg, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} SCRIPTS procesados correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarScripts: Fin creación de Scripts");

            _progresoGeneral += 1;
        }

        private void ProcesarScripts()
        {
            logger.Info("ProcesarScripts: Inicio creación de Scripts");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Scripts";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("SCRIPTS", 2), false);

            List<ScriptFile> files = paquete.ListaScripts;

            int contador = 0;
            int maxContadorParcial = files.Count();

            if (files.Count() > 0)
            {
                foreach (ScriptFile script in files)
                {
                    String mensaje = $"Importando \"{script.FolderPath}/{script.FileName}\"";
                    logger.Info($"ProcesarScripts: {mensaje}");

                    script.Content = ReemplazarPaqueteHANA(script.Content);
                    _SB1DataAccess.AddScript(script, out String errorMsg);

                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, errorMsg, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contador} SCRIPTS procesados", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarScripts: Fin creación de Scripts");

            _progresoGeneral += 1;
        }

        /// <summary>
        /// reemplaza el nombre del paquete predeterminado por el paquete nuevo
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string ReemplazarPaqueteHANA(string contenido)
        {
            if (paquete.TipoBD.CompareTo("HANA") != 0) return contenido;

            string patron = paquete.OldHANAViewPackageName.Replace("\"", "\\\"").Replace(".", "\\.");
            Regex regex = new Regex(patron);
            int cantidad = regex.Matches(contenido).Count;
            if (cantidad > 0)
            {
                string reemplazado = regex.Replace(contenido, paquete.NewHANAViewPackageName);
                logger.Info($"ReemplazarPaqueteHANA: Se han reemplazado {cantidad} coinciencias de \"{paquete.OldHANAViewPackageName}\"");
                return reemplazado;
            }

            return contenido;
        }

        private void ProcesarGEP()
        {
            logger.Info("ProcesarGEP: Inicio creación de Formatos Electrónicos ");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Formatos Electrónicos (GEP)";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("FORMATOS ELECTRÓNICOS (GEP)", 2), false);

            List<FormatoElectronicoWrp> files = paquete.ListaFormatoElectronicos;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = files.Count();

            if (files.Count() > 0)
            {
                foreach (FormatoElectronicoWrp file in files)
                {
                    String resultadoTexto = "";
                    String mensaje = $"Cargando \"{file.O.FileName}\"";
                    logger.Info($"ProcesarGEP: {mensaje}");

                    if (!_SB1DataAccess.AddElectronicFormat(file.O, out String errorMsg))
                    {
                        resultadoTexto = errorMsg;
                    }
                    else contadorOK++;
                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, resultadoTexto, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contador} de {maxContadorParcial} FORMATOS ELECTRÓNICOS (GEP) procesados correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarGEP: Fin creación de formato electrónicos ");

            _progresoGeneral += 1;
        }
        private void ProcesarCrystal()
        {
            logger.Info("ProcesarCrystal: Inicio creación de Formatos Crystal");

            _mensajeGeneral = $"[{paquete.Nombre.ToUpper()}] PASO {_progresoGeneral + 1} de {_maxContadorGeneral}: Procesando Formatos Crystal (RPT)";

            EnviarProgreso(0, 100, "Preparando...", "\n" + DecorarEtiqueta("FORMATOS CRYSTAL", 2), false);

            List<FormatoCrystalWrp> files = paquete.ListaCrystals;

            int contador = 0;
            int contadorOK = 0;
            int maxContadorParcial = files.Count();

            if (files.Count() > 0)
            {
                foreach (FormatoCrystalWrp file in files)
                {
                    String resultadoTexto = "";
                    String mensaje = $"Cargando \"{file.O.MenuPath}/{file.O.Title}\"";
                    logger.Info($"ProcesarCrystal: {mensaje}");

                    if (!_SB1DataAccess.AddCrystalFormat(file.O, out String errorMsg))
                    {
                        resultadoTexto = errorMsg;
                    }
                    else contadorOK++;

                    contador++;
                    EnviarProgreso(contador, maxContadorParcial, mensaje, resultadoTexto, false);
                }
                EnviarProgreso(0, 100, "", DecorarEtiqueta($"{contadorOK} de {maxContadorParcial} FORMATOS CRYSTAL procesados correctamente", 3), false);
            }
            else
            {
                EnviarProgreso(0, 100, "", DecorarEtiqueta("No existen elementos para procesar", 3, false), false);
            }

            logger.Info("ProcesarCrystal: Fin creación de formato crystal ");

            _progresoGeneral += 1;
        }

        /// <summary>
        /// devuelve un texto asignando caracteres ecorativos para estilizar la vista de resultados
        /// </summary>
        /// <param name="texto"></param>
        /// <param name="tipo">1 = paquete, 2 = Elemento, 3 = Fin elemento</param>
        /// <returns></returns>
        private string DecorarEtiqueta(string texto, int tipo, bool tieneElementos = true)
        {
            int mitadTexto = (int)Math.Ceiling(texto.Length / 2.0);
            int mitadMaximo = MaxLineLength / 2;
            string textofinal = texto;

            switch (tipo)
            {
                case 1:
                    textofinal = textofinal.PadLeft(mitadMaximo + mitadTexto, '=').PadRight(MaxLineLength, '=');
                    break;
                case 2:
                    textofinal = textofinal.PadLeft(mitadMaximo + mitadTexto, '-').PadRight(MaxLineLength, '-');
                    break;
                case 3:
                    textofinal = (tieneElementos ? $"> {textofinal} <" : $"({textofinal})");
                    mitadTexto = (int)Math.Ceiling(textofinal.Length / 2.0);
                    char padding = (tieneElementos ? '-' : ' ');
                    textofinal = textofinal.PadLeft(mitadMaximo + mitadTexto, padding).PadRight(MaxLineLength, padding);
                    break;
            }
            return textofinal;
        }
    }
}
