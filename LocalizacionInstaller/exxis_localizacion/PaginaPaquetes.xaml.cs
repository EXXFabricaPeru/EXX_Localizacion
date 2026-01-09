using exxis_localizacion.datasource;
using exxis_localizacion.entidades;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace exxis_localizacion
{
    /// <summary>
    /// Lógica de interacción para PaginaPaquetes.xaml
    /// </summary>
    public partial class PaginaPaquetes : Page, INotifyPropertyChanged
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public event PropertyChangedEventHandler PropertyChanged;

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
        private IResourceGrabber resourceGrabber;
        private MainWindow _frmPrincipal;
        public PaginaPaquetes(MainWindow pFrmPrincipal)
        {
            DataContext = this;
            _frmPrincipal = pFrmPrincipal;
            InitializeComponent();
        }
        public void Mostrar()
        {
            string companyName = _frmPrincipal.SB1DataAccess.SB1Company.CompanyName;
            string companyBD = _frmPrincipal.SB1DataAccess.SB1Company.CompanyDB;
            NombreSociedad = $"{companyName} ({companyBD})";
            _frmPrincipal.Content = this;
            DescargarPaquetes();
        }

        private void DescargarPaquetes()
        {
            resourceGrabber = new GDriveResourceGrabber();
            ListaPaquetes = resourceGrabber.GetPackageList(GetBDType());
        }

        private void btnContinuar_Click(object sender, RoutedEventArgs e)
        {
            List<SB1Package> lista = ListaPaquetes.Where(x => x.Check).ToList();
            if (lista.Count == 0)
            {
                MessageBox.Show("Debe elegir al menos un paquete");
                return;
            }

            _frmPrincipal.PagCargando.Mostrar();
            _frmPrincipal.PagParams.Mostrar(lista);
        }

        private string GetBDType()
        {
            return _frmPrincipal.SB1DataAccess.SB1Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB ? "HANA" : "MSSQL";
        }
    }
}
