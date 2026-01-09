using exxis_localizacion.entidades;
using exxis_localizacion.procesadores;
using exxis_localizacion.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace exxis_localizacion
{
    /// <summary>
    /// Lógica de interacción para PaginaEjecucion.xaml
    /// </summary>
    public partial class PaginaEjecucion : Page, INotifyPropertyChanged
    {
        private MainWindow _frmPrincipal;
        private ScrollViewer txtResultadosScrollViewer;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _nombreSociedad = "";
        public String NombreSociedad
        {
            get { return _nombreSociedad; }
            set
            {
                _nombreSociedad = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NombreSociedad"));
            }
        }

        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);
        GDrivePackageProcessor _procesador;

        private string _progresoGeneralTexto;
        public string ProgresoGeneralTexto {
            get { return _progresoGeneralTexto; }
            set {
                _progresoGeneralTexto = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgresoGeneralTexto"));
            } 
        }
        private string _progresoParcialTexto;
        public string ProgresoParcialTexto {
            get { return _progresoParcialTexto; }
            set
            {
                _progresoParcialTexto = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgresoParcialTexto"));
            }
        }
        private String _textoResultados;
        public String TextoResultados
        {
            get { return _textoResultados; }
            set
            {
                _textoResultados = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextoResultados"));
            }
        }
        public PaginaEjecucion(MainWindow pFrmPrincipal)
        {
            DataContext = this;
            _frmPrincipal = pFrmPrincipal;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            txtResultadosScrollViewer = UIHelper.GetScrollViewer(txtResultados);
        }

        public void Iniciar(List<SB1Package> listaPaquetes)
        {
            string companyName = _frmPrincipal.SB1DataAccess.SB1Company.CompanyName;
            string companyBD = _frmPrincipal.SB1DataAccess.SB1Company.CompanyDB;
            NombreSociedad = $"{companyName} ({companyBD})";
            _frmPrincipal.Content = this;
            _procesador = new GDrivePackageProcessor(_frmPrincipal.SB1DataAccess);
            _procesador.PackageProcessorEventHandler += procesador_EjecutandoProcesoEventHandler;
            new Thread(new ThreadStart(() => {
                PackageProcessorEventArgs ppe = null;
                try
                {
                    foreach(SB1Package pqt in listaPaquetes)
                    {
                        _procesador.EjecutarProcesos(pqt);
                    }
                    ppe = new PackageProcessorEventArgs()
                    {
                        ProgresoGeneral = 100,
                        TextoGeneral = "Finalizado!!!",
                        ProgresoParcial = 0,
                        TextoParcial = "",
                        TextoResultadoParcial = "Proceso finalizado",
                        Finalizado = true,
                    };
                }
                catch (Exception ex)
                {
                    ppe = new PackageProcessorEventArgs()
                    {
                        ProgresoGeneral = 100,
                        TextoGeneral = "Finalizado!!!",
                        ProgresoParcial = 0,
                        TextoParcial = "",
                        TextoResultadoParcial = "Proceso finalizado con errores",
                        Finalizado = true,
                    };
                    logger.Error($"Iniciar", ex);
                }
                finally
                {
                    procesador_EjecutandoProcesoEventHandler(null, ppe);                    
                }
            })).Start();
        }
        private void procesador_EjecutandoProcesoEventHandler(object sender, PackageProcessorEventArgs e)
        {
            this.Dispatcher.Invoke(() => {
                prbProgresoGen.Value = e.ProgresoGeneral;
                ProgresoGeneralTexto = e.TextoGeneral;
                prbProgresoPar.Value = e.ProgresoParcial;
                ProgresoParcialTexto = e.TextoParcial;
                if (!String.IsNullOrEmpty(e.TextoResultadoParcial)) 
                {
                    TextoResultados += e.TextoResultadoParcial + "\n";
                    if(txtResultadosScrollViewer!=null) txtResultadosScrollViewer.ScrollToBottom();
                }
                if (!string.IsNullOrWhiteSpace(e.Vistas))
                {
                    FormModeloVistas fmv = new FormModeloVistas(_frmPrincipal);
                    fmv.Mostrar(e.Vistas);
                }
                if (e.Finalizado)
                {
                    if (txtResultadosScrollViewer != null) txtResultadosScrollViewer.ScrollToBottom();
                    btnSiguiente.IsEnabled = true;
                }
            });
        }

        private void btnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            _frmPrincipal.Close();            
        }
    }
}
