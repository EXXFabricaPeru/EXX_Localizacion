using ExxisBibliotecaClases.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases.query
{
    public interface IQueryExecutor
    {
        event EventHandler<QueryExecutedEventArgs> QueryExecuted;
        List<T> ExecuteQuery<T>(String qryStr);
        T ExecuteSingleResult<T>(String qryStr);
        void ExecuteUpdate(String updStr);
    }
}
