using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlumbingProps.Logger
{
    public class Binnacle
    {
        public static void ProcessEvent(Event _event)
        {
            ILogger log = LogManager.GetCurrentClassLogger();
            switch (_event.category)
            {
                case Event.Category.Information:
                    log.Info(_event.description);
                    break;
                case Event.Category.Warning:
                    log.Warn(_event.description);
                    break;
                case Event.Category.Error:
                    log.Error(_event.description);
                    break;
                default:
                    log.Info(_event.description);
                    break;
            }

        }
    }
}
