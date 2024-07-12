//using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ZOI.DAL.DatabaseUtility.Interface;

namespace ZOI.DAL.DatabaseUtility
{
    public class ADODataFunction //: IADODataFuntion
    {
        public ADODataFunction()
        { }

       // public List<T> ExecSQL<T>(string query)
       // {
       //     try
       //     {
       //         string ConnectionStringSSON = string.Empty;
       //         var configurationBuilder = new ConfigurationBuilder();
       //         var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
       //         configurationBuilder.AddJsonFile(path, false);

       //         var root = configurationBuilder.Build();
       //         ConnectionStringSSON = root.GetConnectionString("DatabaseConnection");

       //         DataSet ds = new DataSet();

       //         using (SqlConnection con = new SqlConnection(ConnectionStringSSON))
       //         {
       //             using (SqlCommand cmd = new SqlCommand(query, con))
       //             {
       //                 /*try
       //                 {*/
       //                 if (con.State == ConnectionState.Closed)
       //                     con.Open();

       //                 cmd.CommandType = CommandType.Text;
       //                 cmd.CommandTimeout = 0;


       //                 using (var result = cmd.ExecuteReader())
       //                 {
       //                     List<T> list = new List<T>();

       //                     while (result.Read())
       //                     {
       //                         T? obj = default(T);
       //                         obj = Activator.CreateInstance<T>();
       //                         for (int i = 0; i < result.FieldCount; i++)
       //                         {
       //                             Type type = obj.GetType();
       //                             PropertyInfo prop = type.GetProperty(result.GetName(i));
       //                             if (!object.Equals(result.GetValue(i), DBNull.Value))
       //                             {
       //                                 prop.SetValue(obj, result.GetValue(i) ?? null, null);
       //                             }
       //                         }

       //                         list.Add(obj);
       //                     }
       //                     return list;


       //                 }
       //                 /*}
       //                 catch (Exception ex)
       //                 {
       //                     // ToDo: Error Handling
       //                     throw ex;
       //                 }
       //                 finally
       //                 {
                            
       //                     if (con.State == ConnectionState.Open)
       //                         con.Close();
       //                     cmd.Dispose();
       //                     con.Dispose();

       //                 }*/
       //             }
       //         }

       //     }
       //     catch (Exception ex)
       //     {
       //         StackTrace CallStack = new StackTrace(ex, true);
       //         ex.Data["ErrDescription"] = ex.Data["ErrDescription"] != null ? ex.Data["ErrDescription"] : string.Format("Error captured in {0} on Line No {1} of Method {2}", CallStack.GetFrame(0).GetFileName(), CallStack.GetFrame(0).GetFileLineNumber(), CallStack.GetFrame(0).GetMethod().ToString());
       //         throw ex;
       //     }

       // }

       // public List<T> ExecSQL<T>(string query, SqlParameter[] param)
       // {
       //     try
       //     {
       //         string ConnectionStringSSON = string.Empty;
       //         var configurationBuilder = new ConfigurationBuilder();
       //         var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
       //         configurationBuilder.AddJsonFile(path, false);

       //         var root = configurationBuilder.Build();
       //         ConnectionStringSSON = root.GetConnectionString("DatabaseConnection");

       //         DataSet ds = new DataSet();

       //         using (SqlConnection con = new SqlConnection(ConnectionStringSSON))
       //         {
       //             using (SqlCommand cmd = new SqlCommand(query, con))
       //             {
       //                 /*try
       //                 {*/
       //                 if (con.State == ConnectionState.Closed)
       //                     con.Open();

       //                 cmd.CommandType = CommandType.StoredProcedure;
       //                 cmd.CommandTimeout = 0;

       //                 if (param != null)
       //                 {
       //                     foreach (SqlParameter item in param)
       //                     {
       //                         cmd.Parameters.Add(item);
       //                     }
       //                 }

       //                 using (var result = cmd.ExecuteReader())
       //                 {
       //                     List<T> list = new List<T>();

       //                     while (result.Read())
       //                     {
       //                         T obj = default(T);
       //                         obj = Activator.CreateInstance<T>();
       //                         for (int i = 0; i < result.FieldCount; i++)
       //                         {
       //                             Type type = obj.GetType();
       //                             PropertyInfo prop = type.GetProperty(result.GetName(i));
       //                             if (!object.Equals(result.GetValue(i), DBNull.Value))
       //                             {
       //                                 prop.SetValue(obj, result.GetValue(i) ?? null, null);
       //                             }
       //                         }

       //                         list.Add(obj);
       //                     }
       //                     return list;


       //                 }
       //                 /*}
       //                 catch (Exception ex)
       //                 {
       //                     // ToDo: Error Handling
       //                     throw ex;
       //                 }
       //                 finally
       //                 {

       //                     if (con.State == ConnectionState.Open)
       //                         con.Close();
       //                     cmd.Dispose();
       //                     con.Dispose();

       //                 }*/
       //             }
       //         }

       //     }
       //     catch (Exception ex)
       //     {
       //         StackTrace CallStack = new StackTrace(ex, true);
       //         ex.Data["ErrDescription"] = ex.Data["ErrDescription"] != null ? ex.Data["ErrDescription"] : string.Format("Error captured in {0} on Line No {1} of Method {2}", CallStack.GetFrame(0).GetFileName(), CallStack.GetFrame(0).GetFileLineNumber(), CallStack.GetFrame(0).GetMethod().ToString());
       //         throw ex;
       //     }

       // }

       // public DataTable ExecuteDataTable(string CommandText, SqlParameter[]? SqlParameters, string dbConnection, CommandType Type = CommandType.StoredProcedure)
       // {
       //     try
       //     {
       //         DataTable dataTable = new DataTable();

       //         using (SqlConnection con = new SqlConnection(dbConnection))
       //         {
       //             using (SqlDataAdapter da = new SqlDataAdapter(CommandText, con))
       //             {
       //                 if (con.State == ConnectionState.Closed)
       //                     con.Open();

       //                 da.SelectCommand.CommandType = Type;
       //                 da.SelectCommand.CommandTimeout = 0;

       //                 if (SqlParameters != null)
       //                 {
       //                     da.SelectCommand.Parameters.AddRange(SqlParameters);
       //                 }
       //                 da.Fill(dataTable);
       //             }
       //         }

       //         return dataTable;
       //     }
       //     catch (Exception ex)
       //     {
       //         StackTrace CallStack = new StackTrace(ex, true);
       //         ex.Data["ErrDescription"] = ex.Data["ErrDescription"] != null ? ex.Data["ErrDescription"] : string.Format("Error captured in {0} on Line No {1} of Method {2}", CallStack.GetFrame(0).GetFileName(), CallStack.GetFrame(0).GetFileLineNumber(), CallStack.GetFrame(0).GetMethod().ToString());
       //         throw ex;
       //     }
       // }

       // public DataSet ExecuteDataset(string CommandText, SqlParameter[] SqlParameters, string dbConnection, CommandType Type = CommandType.StoredProcedure)
       // {
       //     try
       //     {
       //         string ConnectionStringSSON = string.Empty;
       //         var configurationBuilder = new ConfigurationBuilder();
       //         var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
       //         configurationBuilder.AddJsonFile(path, false);

       //         var root = configurationBuilder.Build();
       //         //var a = root.AsEnumerable().Equals(e=>e.Key == "ConnectionStrings");
       //         ConnectionStringSSON = root.GetConnectionString(dbConnection);

       //         DataSet ds = new DataSet();

       //         using (SqlConnection con = new SqlConnection(ConnectionStringSSON))
       //         {
       //             using (SqlDataAdapter da = new SqlDataAdapter(CommandText, con))
       //             {
       //                 /*try
       //                 {*/
       //                 if (con.State == ConnectionState.Closed)
       //                     con.Open();

       //                 da.SelectCommand.CommandType = Type;
       //                 da.SelectCommand.CommandTimeout = 0;

       //                 if (SqlParameters != null)
       //                 {
       //                     da.SelectCommand.Parameters.AddRange(SqlParameters);
       //                 }
       //                 da.Fill(ds);
       //                 /*}
       //                 catch
       //                 {
       //                     // ToDo: Error Handling
       //                     throw;
       //                 }
       //                 finally
       //                 {
       //                     da.SelectCommand.Parameters.Clear();
       //                     if (con.State == ConnectionState.Open)
       //                         con.Close();
       //                     da.Dispose();
       //                     con.Dispose();

       //                 }*/
       //             }
       //         }

       //         return ds;
       //     }
       //     catch (Exception ex)
       //     {
       //         StackTrace CallStack = new StackTrace(ex, true);
       //         ex.Data["ErrDescription"] = ex.Data["ErrDescription"] != null ? ex.Data["ErrDescription"] : string.Format("Error captured in {0} on Line No {1} of Method {2}", CallStack.GetFrame(0).GetFileName(), CallStack.GetFrame(0).GetFileLineNumber(), CallStack.GetFrame(0).GetMethod().ToString());
       //         throw ex;
       //     }
       // }

       // /*
       // public DataSet ExecuteDataset(string CommandText, DbContextConnection connect, SqlParameter[] SqlParameters, CommandType Type = CommandType.StoredProcedure)
       // {
       //     try
       //     {
       //         string ConnectionString = string.Empty;
       //         var configurationBuilder = new ConfigurationBuilder();
       //         var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
       //         configurationBuilder.AddJsonFile(path, false);

       //         var root = configurationBuilder.Build();
       //         ConnectionString = root.GetConnectionString(connect.ToString());

       //         DataSet ds = new DataSet();

       //         using (SqlConnection con = new SqlConnection(ConnectionString))
       //         {
       //             using (SqlDataAdapter da = new SqlDataAdapter(CommandText, con))
       //             {                        
       //                 if (con.State == ConnectionState.Closed)
       //                     con.Open();

       //                 da.SelectCommand.CommandType = Type;
       //                 da.SelectCommand.CommandTimeout = 0;

       //                 if (SqlParameters != null)
       //                 {
       //                     da.SelectCommand.Parameters.AddRange(SqlParameters);
       //                 }
       //                 da.Fill(ds);
       //             }
       //         }

       //         return ds;
       //     }
       //     catch (Exception ex)
       //     {
       //         StackTrace CallStack = new StackTrace(ex, true);
       //         ex.Data["ErrDescription"] = ex.Data["ErrDescription"] != null ? ex.Data["ErrDescription"] : string.Format("Error captured in {0} on Line No {1} of Method {2}", CallStack.GetFrame(0).GetFileName(), CallStack.GetFrame(0).GetFileLineNumber(), CallStack.GetFrame(0).GetMethod().ToString());
       //         throw ex;
       //     }
       // }
       //*/
       // public int ExecuteNonQuery(string CommandText, SqlParameter[] SqlParameters,string dbConnection , CommandType Type = CommandType.StoredProcedure)
       // {
       //     try
       //     {
       //         string ConnectionStringSSON = string.Empty;
       //         var configurationBuilder = new ConfigurationBuilder();
       //         var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
       //         configurationBuilder.AddJsonFile(path, false);

       //         var root = configurationBuilder.Build();
       //         ConnectionStringSSON = root.GetConnectionString(dbConnection);

       //         DataSet ds = new DataSet();

       //         using (SqlConnection con = new SqlConnection(ConnectionStringSSON))
       //         {
       //             using (SqlCommand cmd = new SqlCommand(CommandText, con))
       //             {
       //                 /*try
       //                 {*/
       //                 if (con.State == ConnectionState.Closed)
       //                     con.Open();

       //                 cmd.CommandType = Type;
       //                 cmd.CommandTimeout = 0;

       //                 if (SqlParameters != null)
       //                 {
       //                     cmd.Parameters.AddRange(SqlParameters);
       //                 }
       //                 return cmd.ExecuteNonQuery();
       //                 /*}
       //                 catch (Exception ex)
       //                 {
       //                     // ToDo: Error Handling
       //                     throw ex;
       //                 }
       //                 finally
       //                 {
       //                     cmd.Parameters.Clear();
       //                     if (con.State == ConnectionState.Open)
       //                         con.Close();
       //                     cmd.Dispose();
       //                     con.Dispose();
       //                 }*/
       //             }
       //         }
       //     }
       //     catch (Exception ex)
       //     {
       //         StackTrace CallStack = new StackTrace(ex, true);
       //         ex.Data["ErrDescription"] = ex.Data["ErrDescription"] != null ? ex.Data["ErrDescription"] : string.Format("Error captured in {0} on Line No {1} of Method {2}", CallStack.GetFrame(0).GetFileName(), CallStack.GetFrame(0).GetFileLineNumber(), CallStack.GetFrame(0).GetMethod().ToString());
       //         throw ex;
       //     }
       // }

    }
}
