using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlumbingProps.Exceptions
{
    public class ManagerException
    {
        public string ProcessException(Exception ex)
        {
            string msg = $"Ocurrio un erro al procesar la informacion por favor comuniquese con el administrador de sistemas, error registrado en {DateTime.Now.ToString()}";
            ILogger log = LogManager.GetCurrentClassLogger();
            string msgError = ex.Message;
            int contador = 0;
            while (ex.InnerException != null && contador < 5)
            {
                contador++;
                msgError += " inner -> " + ex.InnerException.Message;
                ex = ex.InnerException;
            }
            msgError += " stactrace -> " + ex.StackTrace;
            log.Error(ex, msgError);

            return msg;
        }
    }
}
