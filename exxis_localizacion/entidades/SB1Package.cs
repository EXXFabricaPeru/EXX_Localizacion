using exxis_localizacion.datasource;
using exxis_localizacion.util;
using ExxisBibliotecaClases.entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace exxis_localizacion.entidades
{
    public class SB1Package: INotifyPropertyChanged
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public event PropertyChangedEventHandler PropertyChanged;

        public const int ERROR = 1;
        public const int WAIT = 2;
        public const int SUCCESS = 3;

        private SAPbobsCOM.Company _sb1Company;

        private string _tipoBD;
        public string TipoBD
        {
            get { return _tipoBD; }
            set
            {
                _tipoBD = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TipoBD"));
            }
        }

        private string _oldHANAViewPackageName = "\"_SYS_BIC\".\"EXX_";
        public string OldHANAViewPackageName { 
            get { return _oldHANAViewPackageName; }
            set 
            {
                _oldHANAViewPackageName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OldHANAViewPackageName"));
            }
        }
        private string _newHANAViewPackageName;
        public string NewHANAViewPackageName
        {
            get { return _newHANAViewPackageName; }
            set
            {
                _newHANAViewPackageName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewHANAViewPackageName"));
            }
        }

        private IResourceGrabber resourceGrabber;

        public bool Check { get; set; } = true;
        public string Nombre { get; set; }
        public string Id { get; set; }
        private int _contadorItems = 0;

        private int _selectedItem = -1; //tab selecionado
        public int SelectedItem { 
            get { return _selectedItem; } 
            set { 
                _selectedItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedItem"));
            }
        }

        private List<TablaUsuarioWrp> _listaTablas;
        public List<TablaUsuarioWrp> ListaTablas { 
            get { return _listaTablas; } 
            set {
                _listaTablas = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaTablas"));
            } 
        }
        public string TablasUsuarioId { get; internal set; }
        public ObjetoStats IndicadorTablas { get; private set; } = new ObjetoStats();
        public System.Windows.Visibility TablasUsuarioExiste
        {
            get
            {
                return (TablasUsuarioId != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { }
        }

        private List<CampoUsuarioWrp> _listaCampos;
        public List<CampoUsuarioWrp> ListaCampos
        {
            get { return _listaCampos; }
            set
            {
                _listaCampos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaCampos"));
            }
        }
        public string CamposUsuarioId { get; internal set; }
        public ObjetoStats IndicadorCampos { get; private set; } = new ObjetoStats();
        public System.Windows.Visibility CamposUsuarioExiste
        {
            get
            {
                return (CamposUsuarioId != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { }
        }
        private CampoUsuarioWrp _selectedCampoUsuario;
        public CampoUsuarioWrp SelectedCampoUsuario
        {
            get { return _selectedCampoUsuario; }
            set
            {
                _selectedCampoUsuario = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedCampoUsuario"));
            }
        }

        private List<ObjetoUsuarioWrp> _listaObjetos;
        public List<ObjetoUsuarioWrp> ListaObjetos
        {
            get { return _listaObjetos; }
            set
            {
                _listaObjetos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaObjetos"));
            }
        }
        public string ObjetosUsuarioId { get; internal set; }
        public ObjetoStats IndicadorObjetos { get; private set; } = new ObjetoStats();
        public System.Windows.Visibility ObjetosUsuarioExiste { 
            get {
                return (ObjetosUsuarioId != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { } 
        }
        public void ObjetosUsuariosChangeMode(int mode, string mensaje = null)
        {            
            switch (mode)
            {
                case ERROR:
                    IndicadorObjetos.SetErrorMode(mensaje);
                    break;
                case WAIT:
                    IndicadorObjetos.SetWatingMode();
                    break;
                case SUCCESS:
                    IndicadorObjetos.SetSucessMode();
                    break;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorObjetos"));
        }
        private ObjetoUsuarioWrp _selectedObjetoUsuario;
        public ObjetoUsuarioWrp SelectedObjetoUsuario
        {
            get { return _selectedObjetoUsuario; }
            set
            {
                _selectedObjetoUsuario = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedObjetoUsuario"));
            }
        }

        private List<DatosTabla> _listaDatosTablas;
        public List<DatosTabla> ListaDatosTablas
        {
            get { return _listaDatosTablas; }
            set
            {
                _listaDatosTablas = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaDatosTablas"));
            }
        }
        public ObjetoStats IndicadorDatosTablas { get; private set; } = new ObjetoStats();
        public string DatosTablasUsuarioId { get; internal set; }
        public System.Windows.Visibility DatosTablasExiste
        {
            get
            {
                return (DatosTablasUsuarioId != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { }
        }

        private List<DatosObjetoWrp> _listaDatosObjetos;
        public List<DatosObjetoWrp> ListaDatosObjetos
        {
            get { return _listaDatosObjetos; }
            set
            {
                _listaDatosObjetos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaDatosObjetos"));
            }
        }
        public string DatosObjetosUsuarioId { get; internal set; }
        public ObjetoStats IndicadorDatosObjetos { get; private set; } = new ObjetoStats();
        public System.Windows.Visibility DatosObjetosExiste
        {
            get
            {
                return (DatosObjetosUsuarioId != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { }
        }
        private int _datosObjetosSelectedIndex;
        public int DatosObjetosSelectedIndex { 
            get { return _datosObjetosSelectedIndex; } 
            set {
                _datosObjetosSelectedIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DatosObjetosSelectedIndex"));
            } 
        }

        private List<CategoriaConsultaWrp> _listaCategorias;
        public List<CategoriaConsultaWrp> ListaCategorias
        {
            get { return _listaCategorias; }
            set
            {
                _listaCategorias = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaCategorias"));
            }
        }
        private List<ConsultaUsuarioWrp> _listaConsultasUsuario;
        public List<ConsultaUsuarioWrp> ListaConsultasUsuario
        {
            get { return _listaConsultasUsuario; }
            set
            {
                _listaConsultasUsuario = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaConsultasUsuario"));
            }
        }
        private List<BusquedaFormateadaWrp> _listaBusquedasFormateadas;
        public List<BusquedaFormateadaWrp> ListaBusquedasFormateadas
        {
            get { return _listaBusquedasFormateadas; }
            set
            {
                _listaBusquedasFormateadas = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaBusquedasFormateadas"));
            }
        }
        public string GestorConsultasId { get; internal set; }
        public ObjetoStats IndicadorGestorConsultas { get; private set; } = new ObjetoStats();
        public System.Windows.Visibility GestorConsultasExiste
        {
            get
            {
                return (GestorConsultasId != null  ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { }
        }
        private ConsultaUsuarioWrp _selectedConsultaUsuario;
        public ConsultaUsuarioWrp SelectedConsultaUsuario
        {
            get { return _selectedConsultaUsuario; }
            set
            {
                _selectedConsultaUsuario = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedConsultaUsuario"));
            }
        }

        private string _modeloVistaPath;
        public string ModeloVistaPath
        {
            get { return _modeloVistaPath; }
            set
            {
                _modeloVistaPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ModeloVistaPath"));
            }
        }
        public string ModeloVistaId { get; internal set; }
        public ObjetoStats IndicadorModeloVistas { get; private set; } = new ObjetoStats();
        public System.Windows.Visibility ModeloVistaExiste
        {
            get
            {
                return (ModeloVistaId != null  && TipoBD.CompareTo("HANA") == 0? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { }
        }

        private List<ScriptFile> _listaScripts;
        public List<ScriptFile> ListaScripts
        {
            get { return _listaScripts; }
            set
            {
                _listaScripts = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaScripts"));
            }
        }

        private List<Script> _listaScriptsNew;
        public List<Script> ListaScriptsNew
        {
            get { return _listaScriptsNew; }
            set
            {
                _listaScriptsNew = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaScripts"));
            }
        }

        public string ScriptsId { get; internal set; }
        public ObjetoStats IndicadorScripts { get; private set; } = new ObjetoStats();
        public System.Windows.Visibility ScriptsExiste
        {
            get
            {
                return (ScriptsId != null? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { }
        }
        private ScriptFile _selectedScript;
        public ScriptFile SelectedScript
        {
            get { return _selectedScript; }
            set
            {
                _selectedScript = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedScript"));
            }
        }

        private List<FormatoElectronicoWrp> _listaFormatoElectronicos;
        public List<FormatoElectronicoWrp> ListaFormatoElectronicos
        {
            get { return _listaFormatoElectronicos; }
            set
            {
                _listaFormatoElectronicos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaFormatoElectronicos"));
            }
        }
        public string FormatosElectronicosId { get; internal set; }
        public ObjetoStats IndicadorGEP { get; private set; } = new ObjetoStats();
        public System.Windows.Visibility FormatosElectronicosExiste
        {
            get
            {
                return (FormatosElectronicosId != null? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { }
        }

        private List<FormatoCrystalWrp> _listaCrystals;
        public List<FormatoCrystalWrp> ListaCrystals
        {
            get { return _listaCrystals; }
            set
            {
                _listaCrystals = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaCrystals"));
            }
        }
        public string ReportesCrystalId { get; internal set; }
        public ObjetoStats IndicadorCrystal { get; private set; } = new ObjetoStats();
        public System.Windows.Visibility CrystalsExiste
        {
            get
            {
                return (ReportesCrystalId != null? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
            }
            set { }
        }

        public bool Listo { 
            get {
                return IndicadorTablas.Listo && IndicadorCampos.Listo && IndicadorObjetos.Listo &&
                    IndicadorDatosTablas.Listo && IndicadorDatosObjetos.Listo && IndicadorGestorConsultas.Listo &&
                    IndicadorModeloVistas.Listo && IndicadorScripts.Listo && IndicadorGEP.Listo && IndicadorCrystal.Listo;
            }
            set { } 
        }
        public void Descargar(string tipobd, SAPbobsCOM.Company sb1Company)
        {
            _tipoBD = tipobd;
            _sb1Company = sb1Company;
            NewHANAViewPackageName = $"\"_SYS_BIC\".\"sap.{getPackageDB(_sb1Company.CompanyDB)}.EXX_";
            _contadorItems = -1;

            resourceGrabber = new GDriveResourceGrabber();

            var paquete = this;
            resourceGrabber.PaqueteActual = paquete;

            DescargarTablas();
            DescargarCampos();
            DescargarObjetos();
            DescargarDatosTablas();
            DescargarDatosObjetos();
            DescargarGestorConsultas();
            DescargarVistas();
            DescargarScripts();
            DescargarGEP();
            DescargarCrystals();
            logger.Debug($"Descargar: {this.Nombre} => {SelectedItem}");
        }
        private object getPackageDB(string companyDB)
        {
            return companyDB.Replace("_", "").ToLower();
        }
        public void DescargarTablas()
        {
            IndicadorTablas.SetWatingMode();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorTablas"));
            _contadorItems++;
            if (SelectedItem == -1 && TablasUsuarioId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    ListaTablas = resourceGrabber.GetUserTableList();
                    IndicadorTablas.SetSucessMode();
                }
                catch (Exception ex)
                {
                    IndicadorTablas.SetErrorMode(ex.Message);
                    logger.Error("DescargarTablas", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorTablas"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void DescargarCampos()
        {
            IndicadorCampos.SetWatingMode();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorCampos"));
            _contadorItems++;
            if (SelectedItem == -1 && CamposUsuarioId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    ListaCampos = resourceGrabber.GetUserFieldList();
                    IndicadorCampos.SetSucessMode();
                }
                catch (Exception ex)
                {
                    IndicadorCampos.SetErrorMode(ex.Message);
                    logger.Error("DescargarCampos", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorCampos"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void DescargarObjetos()
        {
            ObjetosUsuariosChangeMode(WAIT);
            _contadorItems++;
            if (SelectedItem == -1 && ObjetosUsuarioId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    ListaObjetos = resourceGrabber.GetUserObjectList();
                    ObjetosUsuariosChangeMode(SUCCESS);
                }
                catch (Exception ex)
                {
                    ObjetosUsuariosChangeMode(ERROR, ex.Message);
                    logger.Error("DescargarObjetos", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void DescargarDatosTablas()
        {
            IndicadorDatosTablas.SetWatingMode();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorDatosTablas"));
            _contadorItems++;
            if (SelectedItem == -1 && DatosTablasUsuarioId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    //Paquete.ListaDatosTablas = resourceGrabber.GetUserObjectsData(_frmPrincipal.SB1DataAccess.SB1Company);
                    ListaDatosTablas = new List<DatosTabla>();
                    throw new Exception("Datos no disponibles");
                    IndicadorDatosTablas.SetSucessMode();
                }
                catch (Exception ex)
                {
                    IndicadorDatosTablas.SetErrorMode(ex.Message);
                    logger.Error("DescargarDatosTablas", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorDatosTablas"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void DescargarDatosObjetos()
        {
            IndicadorDatosObjetos.SetWatingMode();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorDatosObjetos"));
            _contadorItems++;
            if (SelectedItem == -1 && DatosObjetosUsuarioId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    ListaDatosObjetos = resourceGrabber.GetUserObjectsData(_sb1Company);
                    IndicadorDatosObjetos.SetSucessMode();
                    DatosObjetosSelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    IndicadorDatosObjetos.SetErrorMode(ex.Message);
                    logger.Error("DescargarObjetos", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorDatosObjetos"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void DescargarGestorConsultas()
        {
            IndicadorGestorConsultas.SetWatingMode();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorGestorConsultas"));
            _contadorItems++;
            if (SelectedItem == -1 && GestorConsultasId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    ListaCategorias = resourceGrabber.GetQueryCategoryList();
                    ListaConsultasUsuario = resourceGrabber.GetUserQueryList(ListaCategorias, _tipoBD);
                    ListaBusquedasFormateadas = resourceGrabber.GetFormattedSearchList(ListaConsultasUsuario);
                    IndicadorGestorConsultas.SetSucessMode();
                }
                catch (Exception ex)
                {
                    IndicadorGestorConsultas.SetErrorMode(ex.Message);
                    logger.Error("DescargarGestorConsultas", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorGestorConsultas"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void DescargarVistas()
        {
            IndicadorModeloVistas.SetWatingMode();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorModeloVistas"));
            _contadorItems++;
            if (SelectedItem == -1 && ModeloVistaId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    ModeloVistaPath = resourceGrabber.GetModeloVistas();
                    IndicadorModeloVistas.SetSucessMode();
                }
                catch (Exception ex)
                {
                    IndicadorModeloVistas.SetErrorMode(ex.Message);
                    logger.Error("DescargarVistas", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorModeloVistas"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void DescargarScripts()
        {
            IndicadorScripts.SetWatingMode();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorScripts"));
            _contadorItems++;
            if (SelectedItem == -1 && ScriptsId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    ListaScriptsNew = resourceGrabber.GetScriptListNew(_tipoBD);
                    //ListaScripts = resourceGrabber.GetScriptList(_tipoBD);
                    IndicadorScripts.SetSucessMode();
                }
                catch (Exception ex)
                {
                    IndicadorScripts.SetErrorMode(ex.Message);
                    logger.Error("DescargarScripts", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorScripts"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void DescargarGEP()
        {
            IndicadorGEP.SetWatingMode();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorGEP"));
            _contadorItems++;
            if (SelectedItem == -1 && FormatosElectronicosId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    ListaFormatoElectronicos = resourceGrabber.GetElectronicFormatList();
                    IndicadorGEP.SetSucessMode();
                }
                catch (Exception ex)
                {
                    IndicadorGEP.SetErrorMode(ex.Message);
                    logger.Error("DescargarGEP", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorGEP"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void DescargarCrystals()
        {
            IndicadorCrystal.SetWatingMode();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorCrystal"));
            _contadorItems++;
            if (SelectedItem == -1 && ReportesCrystalId != null) SelectedItem = _contadorItems;
            Thread T = new Thread(new ThreadStart(() => {
                try
                {
                    ListaCrystals = resourceGrabber.GetCrystalFormatList(_tipoBD);
                    IndicadorCrystal.SetSucessMode();
                }
                catch (Exception ex)
                {
                    IndicadorCrystal.SetErrorMode(ex.Message);
                    logger.Error("DescargarCrytals", ex);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndicadorCrystal"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }));
            T.Start();
        }
        public void AbrirUbicacionModeloVistas()
        {
            try
            {
                string path = Path.GetDirectoryName(ModeloVistaPath);

                Process.Start(path);

            }catch(Exception ex)
            {
                logger.Error("AbrirUbicacionModeloVistas", ex);
            }
        }
    }
}
