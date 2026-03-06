using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.entidades
{
    public class DBDataSourceOptz
    {
        DBDataSource _ds;
        System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
        System.Globalization.DateTimeFormatInfo dtfi = new System.Globalization.DateTimeFormatInfo();

        public DBDataSource Source { get { return _ds; }}
        public int Size
        {
            get {
                return _ds.Size;
            }
        }
        public DBDataSourceOptz(ref DBDataSource ds) {
            _ds = ds;
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = ",";
        }
        public void SetValue(object Index,int RecordNumber, object newVal) {
            string newValStr;
            if (newVal is double)
            {
                newValStr = ((double)newVal).ToString(nfi);
            }
            else if (newVal is DateTime)
            {
                newValStr = ((DateTime)newVal).ToString("yyyyMMdd");
            }
            else
            {
                newValStr = (newVal?.ToString());
            }           

            _ds.SetValue(Index, RecordNumber, newValStr);
        }
        public String GetValue(object Index, int RecordNumber) {
            return _ds.GetValue(Index, RecordNumber);
        }
        public double GetDoubleValue(object Index, int RecordNumber)
        {            
            String retStr = _ds.GetValue(Index, RecordNumber).ToString();
            if (!string.IsNullOrEmpty(retStr))
            {
                double numero;
                if (double.TryParse(retStr, System.Globalization.NumberStyles.Number, nfi, out numero)) {
                    return numero;
                }
            }

            return 0;
        }
        public int GetIntValue(object Index, int RecordNumber)
        {
            String retStr = _ds.GetValue(Index, RecordNumber).ToString();
            if (!string.IsNullOrEmpty(retStr))
            {
                int numero;
                if (Int32.TryParse(retStr, out numero))
                {
                    return numero;
                }
            }

            return 0;
        }
        public DateTime GetDateValue(object Index, int RecordNumber)
        {
            String retStr = _ds.GetValue(Index, RecordNumber).ToString();
            if (!string.IsNullOrEmpty(retStr))
            {
                DateTime fecha;
                if (DateTime.TryParseExact(retStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out fecha))
                {
                    return fecha;
                }
            }

            return DateTime.MinValue;
        }
        public void RemoveRecord(int RecordNumber)
        {
            _ds.RemoveRecord(RecordNumber);
        }
    }
}
