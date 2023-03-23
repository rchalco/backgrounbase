using CoreAccesLayer.Interface;
using CoreAccesLayer.Wraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.SqlClient;
using PlumbingProps.CrossUtil;
using System.Collections;

namespace CoreAccesLayer.Implement.SQLServer
{
    public class MSSQLRepository<TDbContext> : IRepository where TDbContext : DbContext, new()
    {
        #region  variables
        private bool isAliasConexionString = true;
        private string conexionString = string.Empty;
        public Action<IRepository> ReleaseRepository { get; set; }
        public string IdRespository { get; set; }
        SqlConnection sqlConnection = null;
        private IDbContextTransaction _transaction = null;

        public bool IsAvailable { get; set; }

        public MSSQLRepository()
        {

        }

        protected TDbContext _Context;

        protected TDbContext Context
        {
            get
            {
                if (_Context == null)
                {
                    try
                    {
                        _Context = isAliasConexionString ? new TDbContext() : (TDbContext)Activator.CreateInstance(typeof(TDbContext));
                    }
                    catch (Exception ex)
                    {
                        Exception exBuildContext = new Exception("No se puede instanciar el contexto porque no cuenta con un constructor que reciba la cadena de conexion como parametro", ex);
                        throw exBuildContext;
                    }
                    Context.Database.SetCommandTimeout(7200);
                }

                return _Context;
            }
        }

        #endregion
        public bool CallProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new()
        {
            Type type = Context.GetType();
            Object obj = Activator.CreateInstance(type);
            MethodInfo methodInfo = type.GetMethod(nameProcedure);
            if (methodInfo != null)
            {
                methodInfo.Invoke(obj, parameters);
                if (ReleaseRepository != null)
                {
                    ReleaseRepository(this);
                }
            }
            else
            {
                CallProcedureADO(nameProcedure, parameters);
            }
            return true;
        }

        public bool Commit()
        {
            if (_transaction != null)
            {
                lock (_transaction)
                {
                    //Context.SaveChanges();
                    _transaction.Commit();
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
            if (ReleaseRepository != null)
            {
                ReleaseRepository(this);
            }
            return true;
        }

        public List<T> Getall<T>() where T : class, new()
        {
            List<T> resul = _Context.Set<T>().ToList<T>();
            if (ReleaseRepository != null)
            {
                ReleaseRepository(this);
            }
            return resul;
        }

        public List<T> GetDataByProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new()
        {
            List<T> lResul = new List<T>();
            Type type = Context.GetType();
            Object obj = Activator.CreateInstance(type);
            MethodInfo methodInfo = type.GetMethod(nameProcedure);
            ///Si es un entity que no esta tipado no obtendra al SP
            if (methodInfo == null)
            {
                lResul = GetListByProcedureADO<T>(nameProcedure, parameters);
            }
            else
            {
                object resul = methodInfo.Invoke(obj, parameters);
                methodInfo = typeof(Enumerable).GetMethod("ToList");
                MethodInfo genericMethod = methodInfo.MakeGenericMethod(typeof(T));
                object[] args = { resul };
                lResul = genericMethod.Invoke(resul, args) as List<T>;
            }
            if (ReleaseRepository != null)
            {
                ReleaseRepository(this);
            }
            return lResul;
        }

        public bool Rollback()
        {
            if (_transaction != null)
            {
                lock (_transaction)
                {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                }

            }
            if (ReleaseRepository != null)
            {
                ReleaseRepository(this);
            }
            return true;
        }

        public bool SaveObject<T>(Entity<T> entity) where T : class, new()
        {
            if (entity == null)
            {
                throw new Exception("La entidad ingresada para el registro no puede se nula");
            }
            else if (entity.stateEntity == StateEntity.none)
            {
                throw new ArgumentException("no se definio un estado para la entidad");
            }
            else if (entity.EntityDB == null)
            {
                throw new ArgumentException("no se tiene una entidad valida!, entidad interna nula");
            }

            try
            {

                StateEntity _operation = entity.stateEntity;
                ///si no existe una transaccion abierta entonces crea una nueva
                lock (Context)
                {
                    if (_transaction == null)
                    {
                        _transaction = Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                    }
                }

                lock (_transaction)
                {

                    switch (_operation)
                    {
                        case StateEntity.add:
                            Context.Add(entity.EntityDB);
                            break;

                        case StateEntity.modify:
                            Context.Update(entity.EntityDB);
                            break;

                        case StateEntity.remove:
                            Context.Remove(entity.EntityDB);
                            break;

                        default:
                            break;
                    }
                    Context.SaveChanges();

                }
            }
            catch
            {
                throw;
            }
            return true;
        }

        private object[] GetKeysFromEntity<T>(T entity)
        {
            List<object> resul = new List<object>();
            Context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(x => x.Name).ToList().ForEach(keyName =>
                {
                    resul.Add(entity.GetType().GetProperty(keyName).GetValue(entity, null));
                });

            return resul.ToArray();
        }

        public List<T> SimpleSelect<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            return Context.Set<T>().Where(predicate).ToList<T>();
        }

        private void CallProcedureADO(string nameProcedure, params Object[] param)
        {
            string sqlLog = string.Empty;
            try
            {
                if (_transaction == null && Context.Database.CanConnect())
                {
                    _transaction = Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                }

                lock (_transaction)
                {
                    string commandText = string.Empty;

                    commandText = "exec " + nameProcedure + " ";
                    string nomParametro = "parametro";
                    int a = 0;
                    Dictionary<int, SqlParameter> parametroSalida = new Dictionary<int, SqlParameter>();

                    List<SqlParameter> pameters = new List<SqlParameter>();

                    foreach (object item in param)
                    {
                        commandText += "@" + nomParametro + a.ToString();

                        SqlParameter parametro = null;

                        if (item == null)
                        {
                            parametro = new SqlParameter
                            {
                                ParameterName = "@" + nomParametro + a.ToString(),
                                Value = DBNull.Value
                            };
                            a++;
                            pameters.Add(parametro);
                            commandText += ", ";
                            continue;
                        }
                        parametro = new SqlParameter
                        {
                            ParameterName = "@" + nomParametro + a.ToString(),
                            Direction = item.GetType() == typeof(ParamOut) ? ParameterDirection.Output : ParameterDirection.Input
                        };

                        if (item.GetType() == typeof(ParamOut))
                        {
                            switch ((item as ParamOut).InOut)
                            {
                                case ParamOut.Type.Out:
                                    parametro.Direction = ParameterDirection.Output;
                                    break;

                                case ParamOut.Type.InOut:
                                    parametro.Direction = ParameterDirection.InputOutput;
                                    break;

                                case ParamOut.Type.In:
                                    parametro.Direction = ParameterDirection.Input;
                                    break;

                                default:
                                    parametro.Direction = ParameterDirection.Input;
                                    break;
                            }
                        }
                        string typeParameter = item.GetType().Equals(typeof(ParamOut)) ? (item as ParamOut).Valor.GetType().Name : item.GetType().Name;
                        switch (typeParameter)
                        {
                            case "Int32":
                                parametro.SqlDbType = SqlDbType.Int;
                                parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                                break;

                            case "Int64":
                                parametro.SqlDbType = SqlDbType.BigInt;
                                parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                                break;

                            case "Decimal":
                                parametro.SqlDbType = SqlDbType.Decimal;
                                parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                                break;

                            case "String":
                                parametro.SqlDbType = SqlDbType.VarChar;
                                parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                                break;

                            case "Boolean":
                                parametro.SqlDbType = SqlDbType.Bit;
                                parametro.Value = Convert.ToBoolean((item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item));
                                break;

                            case "DateTime":
                                parametro.SqlDbType = SqlDbType.DateTime;
                                parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                                break;
                            case "Byte[]":
                                parametro.SqlDbType = SqlDbType.VarBinary;
                                parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                                break;
                            default:
                                break;
                        }

                        if (item.GetType() == typeof(ParamOut))
                        {
                            parametro.Size = (item as ParamOut).Size;
                            parametroSalida.Add(a, parametro);
                            commandText += " out,";
                        }
                        else if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(List<>))
                        {
                            parametro.SqlDbType = SqlDbType.Structured;
                            parametro.TypeName = item.GetType().GetGenericArguments()[0].Name;
                            parametro.Value = CustomListExtension.ToDataTable((item as IList), item.GetType().GetGenericArguments()[0].UnderlyingSystemType);
                            commandText += " ,";
                        }
                        else
                        {
                            commandText += ",";
                        }

                        a++;
                        pameters.Add(parametro);
                    }

                    sqlLog = commandText = commandText.Substring(0, commandText.Length - 1);//quitamos la ultima coma

                    _Context.Database.ExecuteSqlRaw(commandText, pameters.ToArray());
                    foreach (var item in parametroSalida)
                    {
                        (param[item.Key] as ParamOut).Valor = item.Value.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Exception excepcion = new Exception("Error en la ejecucion del procedimiento almacenado: " + nameProcedure + "( " + sqlLog + " )", ex);
                throw excepcion;
            }



        }

        private List<T> GetListByProcedureADO<T>(string nameProcedure, params Object[] param) where T : class, new()
        {
            List<T> vListado = new List<T>();
            string sqlLog = string.Empty;
            try
            {
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.CommandText = nameProcedure;

                //gConnection = new SqlConnection(Context.Database.Connection.ConnectionString);
                SqlConnection sqlConnection = (SqlConnection)Context.Database.GetDbConnection();
                sqlCommand.Connection = sqlConnection;

                sqlCommand.CommandText = "exec " + nameProcedure + " ";
                string nomParametro = "parametro";
                int a = 0;
                Dictionary<int, SqlParameter> parametroSalida = new Dictionary<int, SqlParameter>();

                foreach (object item in param)
                {
                    sqlCommand.CommandText += "@" + nomParametro + a.ToString();

                    SqlParameter parametro = null;

                    if (item == null)
                    {
                        parametro = new SqlParameter
                        {
                            ParameterName = "@" + nomParametro + a.ToString(),
                            Value = null
                        };
                        a++;
                        sqlCommand.Parameters.Add(parametro);
                        sqlCommand.CommandText += ",";
                        continue;
                    }

                    parametro = new SqlParameter
                    {
                        ParameterName = "@" + nomParametro + a.ToString(),
                        Direction = item.GetType() == typeof(ParamOut) ? ParameterDirection.Output : ParameterDirection.Input
                    };

                    if (item.GetType() == typeof(ParamOut))
                    {
                        switch ((item as ParamOut).InOut)
                        {
                            case ParamOut.Type.Out:
                                parametro.Direction = ParameterDirection.Output;
                                break;

                            case ParamOut.Type.InOut:
                                parametro.Direction = ParameterDirection.InputOutput;
                                break;

                            case ParamOut.Type.In:
                                parametro.Direction = ParameterDirection.Input;
                                break;

                            default:
                                parametro.Direction = ParameterDirection.Input;
                                break;
                        }
                    }
                    string typeParameter = item.GetType().Equals(typeof(ParamOut)) ? (item as ParamOut).Valor.GetType().Name : item.GetType().Name;
                    switch (typeParameter)
                    {
                        case "Int32":
                            parametro.SqlDbType = SqlDbType.Int;
                            parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                            break;

                        case "Int64":
                            parametro.SqlDbType = SqlDbType.BigInt;
                            parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                            break;

                        case "Decimal":
                            parametro.SqlDbType = SqlDbType.Decimal;
                            parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                            break;

                        case "String":
                            parametro.SqlDbType = SqlDbType.VarChar;
                            parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                            break;

                        case "Boolean":
                            parametro.SqlDbType = SqlDbType.Bit;
                            parametro.Value = Convert.ToBoolean((item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item));
                            break;

                        case "DateTime":
                            parametro.SqlDbType = SqlDbType.Date;
                            parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                            break;
                        case "Byte[]":
                            parametro.SqlDbType = SqlDbType.VarBinary;
                            parametro.Value = item.GetType() == typeof(ParamOut) ? (item as ParamOut).Valor : item;
                            break;

                        default:
                            break;
                    }
                    if (item.GetType() == typeof(ParamOut))
                    {
                        parametro.Size = (item as ParamOut).Size;
                        parametroSalida.Add(a, parametro);
                        sqlCommand.CommandText += " out,";
                    }
                    else if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(List<>))
                    {
                        parametro.SqlDbType = SqlDbType.Structured;
                        parametro.TypeName = item.GetType().GetGenericArguments()[0].Name;
                        parametro.Value = CustomListExtension.ToDataTable((item as IList), item.GetType().GetGenericArguments()[0].UnderlyingSystemType);
                        sqlCommand.CommandText += " ,";
                    }
                    else
                    {
                        sqlCommand.CommandText += ",";
                    }
                    a++;
                    sqlCommand.Parameters.Add(parametro);
                }
                sqlLog = sqlCommand.CommandText = sqlCommand.CommandText.Substring(0, sqlCommand.CommandText.Length - 1);


                //IDataReader dr = GetList(gCommand);
                DataTable dtResul = new DataTable();


                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                dtResul.BeginLoadData();
                da.Fill(dtResul);
                dtResul.EndLoadData();

                foreach (DataRow item in dtResul.Rows)
                {
                    vListado.Add(LoadObject<T>(item));
                }

                DisposeConnection();
                foreach (var item in parametroSalida)
                {
                    (param[item.Key] as ParamOut).Valor = item.Value.Value;
                }

                if (ReleaseRepository != null)
                {
                    ReleaseRepository(this);
                }
            }
            catch (Exception ex)
            {
                Exception excepcion = new Exception("Error en la ejecucion del procedimiento almacenado: " + nameProcedure + "( " + sqlLog + " )", ex);
                throw excepcion;
            }


            return vListado;
        }

        private void DisposeConnection()
        {
            try
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State != ConnectionState.Closed)
                        sqlConnection.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        private T LoadObject<T>(DataRow dr) where T : class, new()
        {
            T vEntity = new T();
            var vPropiedades = vEntity.GetType().GetProperties();

            try
            {
                PropertyInfo[] propiedades = new PropertyInfo[vEntity.GetType().GetProperties().Count()];
                vEntity.GetType().GetProperties().CopyTo(propiedades, 0);


                foreach (PropertyInfo item in propiedades)
                {

                    if (!dr.Table.Columns.Cast<DataColumn>().Any(a => a.ColumnName.Equals(item.Name)))
                    {
                        continue;
                    }
                    if (dr[item.Name] == null ||
                        dr[item.Name] == DBNull.Value)
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, null, null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Int32"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToInt32(dr[item.Name].ToString()), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Int64"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToInt64(dr[item.Name].ToString()), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Decimal"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToDecimal(dr[item.Name]), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Single"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToSingle(dr[item.Name]), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("StringBuilder"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, new StringBuilder(dr[item.Name].ToString()), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("String"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, dr[item.Name].ToString(), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Boolean"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToBoolean(dr[item.Name]), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("DateTime"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToDateTime(dr[item.Name]), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.ToLower().Contains("byte"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, (byte[])dr[item.Name], null);
                    }
                }
            }
            catch
            {
                throw;
            }

            return (T)vEntity;
        }
    }
}


