using CoreAccesLayer.Implement;
using CoreAccesLayer.Implement.MySQL;
using CoreAccesLayer.Implement.SQLServer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreAccesLayer.Interface
{
    public static class FactoryDataInterfaz
    {
        public static IRepository CreateRepository<T>(string provider) where T : DbContext, new()
        {
            IRepository repository = null;
            switch (provider)
            {
                case "mysql": repository = new MySQLRepository<T>(); break;
                case "sqlserver": repository = new MSSQLRepository<T>(); break;
                default:
                    break;
            }
            return repository;
        }

    }
}
