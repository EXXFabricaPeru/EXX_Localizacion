using exxis_localizacion.datasource;
using exxis_localizacion.entidades;
using exxis_localizacion.util;
using ExxisBibliotecaClases.entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;

namespace exxis_localizacion
{
    /// <summary>
    /// Lógica de interacción para PaginaParams.xaml
    /// </summary>
    public partial class PaginaParams : Page, INotifyPropertyChanged
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

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
        private List<SB1Package> _listaPaquetes = new List<SB1Package>();
        public List<SB1Package> ListaPaquetes
        {
            get { return _listaPaquetes; }
            set
            {
                _listaPaquetes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListaPaquetes"));
            }
        }
        private bool _listo;
        public bool Listo
        {
            get { return _listo; }
            set
            {
                _listo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Listo"));
            }
        }
        private MainWindow _frmPrincipal;

        public PaginaParams(MainWindow pFrmPrincipal)
        {
            DataContext = this;
            _frmPrincipal = pFrmPrincipal;
            InitializeComponent();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Validar();
                foreach (SB1Package pqt in ListaPaquetes)
                {
                    pqt.ListaTablas = pqt.ListaTablas.Where(x => x.Check).ToList();
                    pqt.ListaCampos = pqt.ListaCampos.Where(x => x.Check).ToList();
                    pqt.ListaObjetos = pqt.ListaObjetos.Where(x => x.Check).ToList();
                    pqt.ListaDatosObjetos = pqt.ListaDatosObjetos.Where(x => x.Check).ToList();
                    pqt.ListaCategorias = pqt.ListaCategorias.Where(x => x.Check).ToList();
                    pqt.ListaConsultasUsuario = pqt.ListaConsultasUsuario.Where(x => x.Check).ToList();
                    pqt.ListaBusquedasFormateadas = pqt.ListaBusquedasFormateadas.Where(x => x.Check).ToList();
                    pqt.ListaFormatoElectronicos = pqt.ListaFormatoElectronicos.Where(x => x.Check).ToList();
                    pqt.ListaCrystals = pqt.ListaCrystals.Where(x => x.Check).ToList();
                    pqt.ListaScriptsNew = pqt.ListaScriptsNew.Where(x => x.Check).ToList();
                    //pqt.ListaScripts = pqt.ListaScripts.Where(x => x.Check).ToList();
                }

                _frmPrincipal.PagEjecucion.Iniciar(ListaPaquetes);
            }
            catch (Exception ex)
            {
                logger.Error($"Button_Click_1: {ex.Message}");
                System.Windows.MessageBox.Show(_frmPrincipal, ex.Message, "Parámetros", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Validar()
        {

        }
        public void Mostrar(List<SB1Package> listaPaquetes)
        {
            Thread T = new Thread(new ThreadStart(() =>
            {
                ListaPaquetes = listaPaquetes;
                string companyName = _frmPrincipal.SB1DataAccess.SB1Company.CompanyName;
                string companyBD = _frmPrincipal.SB1DataAccess.SB1Company.CompanyDB;
                NombreSociedad = $"{companyName} ({companyBD})";


                foreach (SB1Package pqt in ListaPaquetes)
                {
                    pqt.Descargar(GetBDType(), _frmPrincipal.SB1DataAccess.SB1Company);
                    logger.Debug($"Mostrar: descargando paquete = {pqt.Nombre}");
                }

                //Verificar que hayan cargado todos los paquetes
                System.Timers.Timer t = new System.Timers.Timer(500);
                t.Elapsed += new System.Timers.ElapsedEventHandler((object obj, System.Timers.ElapsedEventArgs args) =>
                {
                    if (ListaPaquetes == null || ListaPaquetes.Count == 0)
                    {
                        Listo = false;
                    }
                    bool listo = true;
                    foreach (SB1Package sb1p in ListaPaquetes)
                    {
                        if (!sb1p.Listo)
                        {
                            listo = false;
                            break;
                        }
                    }
                    if (listo)
                    {
                        t.Stop();
                        t.Dispose();
                    }
                    Listo = listo;
                });
                t.Start();

                Thread.Sleep(4000);

                this.Dispatcher.Invoke(() =>
                {
                    _frmPrincipal.Content = this;
                });
            }));
            T.Start();
        }

        private string GetBDType()
        {
            return _frmPrincipal.SB1DataAccess.SB1Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB ? "HANA" : "MSSQL";
        }
    }
}
