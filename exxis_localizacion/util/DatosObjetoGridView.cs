using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace exxis_localizacion.util
{
    public class DatosObjetoGridView : DataGrid
    {
        public static readonly DependencyProperty ColumnHeadersProperty = DependencyProperty.Register("ColumnHeaders", typeof(ObservableCollection<string>), typeof(DatosObjetoGridView), new PropertyMetadata(new PropertyChangedCallback(OnColumnsChanged)));
        
        public ObservableCollection<string> ColumnHeaders
        {
            get { return GetValue(ColumnHeadersProperty) as ObservableCollection<string>; }
            set { SetValue(ColumnHeadersProperty, value); }
        }

        public string SourceField { get; set; } = "";

        static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as DatosObjetoGridView;
            dataGrid.Columns.Clear();
            if (dataGrid.ColumnHeaders == null) return;
            foreach (var value in dataGrid.ColumnHeaders)
            {
                var column = new DataGridTextColumn() { 
                    Header = value, 
                    CanUserSort = false,
                    Binding = new Binding(dataGrid.SourceField) { 
                        ConverterParameter = value, 
                        Converter = new RegistroDictionaryConverter() 
                    } 
                };
                dataGrid.Columns.Add(column);
            }
        }
    }
}
