using Business.Main.Base;
using CoreAccesLayer.Implement.SQLServer;
using Domain.Main.Wraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Main.Managers
{
    public class ManagerSample : BaseManager
    {

        public Response SampleMethod()
        {

            Response response = new Response { Message = "¨sample ejecutado correctamente", State = ResponseType.Success };
            try
            {
                ParamOut paramOutRespuesta = new ParamOut(true);
                ParamOut paramOutLogRespuesta = new ParamOut("");
                paramOutLogRespuesta.Size = 100;
                //response.ListEntities = repositoryMicroventas.GetDataByProcedure<ResulSPProductosCantidad>("spProductosCantidad", requestSearchProduct.IdEmpresa, "%");
            }
            catch (Exception ex)
            {
                ProcessError(ex, response);
            }
            return response;
        }

    }
}
