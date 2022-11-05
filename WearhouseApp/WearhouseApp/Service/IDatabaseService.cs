using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using WarehouseApp.Model;
using WarerhouseApp.Model;

namespace WarehouseApp.Service
{
    public interface IDatabaseService
    {
        Task<int> RegisterProductAsync(ProductRegistration product);
        Task<int> RegisterProductByStoredProcedureAsync(ProductRegistration product);
    }

    public class SqlDatabaseService : IDatabaseService
    {
        private IConfiguration _configuration;

        public SqlDatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> RegisterProductAsync(ProductRegistration productRegistration)
        {
            int id = -1;
            using (SqlConnection connection = new(_configuration.GetConnectionString("ProductionDb")))
            {
                using var cmd = new SqlCommand("SELECT IdProduct,Name,Description,Price FROM Product WHERE IdProduct=@idproduct", connection);
                cmd.Parameters.AddWithValue("@idproduct", SqlDbType.Int).Value = productRegistration.IdProduct;

                await connection.OpenAsync();
                DbTransaction tran = await connection.BeginTransactionAsync();
                cmd.Transaction = (SqlTransaction)tran;

                try
                {
                    Product prod = null;
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {

                        while (await dr.ReadAsync())
                        {
                            prod = (new Product
                            {
                                IdProduct = dr.GetInt32(dr.GetOrdinal("IdProduct")),
                                Name = dr["Name"].ToString(),
                                Description = dr["Description"].ToString(),
                                Price = Convert.ToDouble(dr["Price"].ToString())
                            });

                        }
                    }

                    if (prod == null)
                    {
                        throw new Exception("Produkt o podanym id nie istnieje");
                    }
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT IdWarehouse,Name,Address FROM Warehouse WHERE IdWarehouse=@idwarehouse";
                    cmd.Parameters.AddWithValue("@idwarehouse", SqlDbType.Int).Value = productRegistration.IdWarehouse;

                    Warehouse warehouse = null;

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            warehouse = (new Warehouse
                            {
                                IdWarehouse = dr.GetInt32(dr.GetOrdinal("IdWarehouse")),
                                Name = dr["Name"].ToString(),
                                Address = dr["Address"].ToString()
                            }); ;

                        }
                    }


                    if (warehouse == null)
                    {
                        throw new Exception("Hurtownia o podanym id nie istnieje");
                    }

                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT IdOrder,IdProduct,Amount,CreatedAt FROM [ORDER] WHERE IdProduct=@idprodukt AND " +
                        "Amount=@amount AND CreatedAt<@createdat";
                    cmd.Parameters.AddWithValue("@idprodukt", SqlDbType.Int).Value = productRegistration.IdProduct;
                    cmd.Parameters.AddWithValue("@amount", SqlDbType.Int).Value = productRegistration.Amount;
                    cmd.Parameters.AddWithValue("@createdat", SqlDbType.DateTime).Value = productRegistration.CreatedAt;

                    Order order = null;
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            order = (new Order
                            {
                                IdOrder = dr.GetInt32(dr.GetOrdinal("IdOrder")),
                                IdProduct = dr.GetInt32(dr.GetOrdinal("IdProduct")),
                                Amount = dr.GetInt32(dr.GetOrdinal("Amount")),
                                CreatedAt = (DateTime)dr["CreatedAt"]
                            });
                        }
                    }

                    if (order == null)
                    {
                        throw new Exception("Zamówienie nie istnieje");
                    }


                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT counter=COUNT(*) FROM Product_Warehouse WHERE IdOrder=@idorder";
                    cmd.Parameters.AddWithValue("@idorder", SqlDbType.Int).Value = order.IdOrder;
                    int count = 0;

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            count = dr.GetInt32(dr.GetOrdinal("counter"));
                        }
                    }

                    if (count != 0)
                    {
                        throw new Exception("Zlecenie było już zrealizowane");
                    }

                    cmd.Parameters.Clear();
                    cmd.CommandText = "UPDATE [Order] SET FulfilledAt=@fulfilledat WHERE IdOrder=@orderid";
                    cmd.Parameters.AddWithValue("@fulfilledat", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.AddWithValue("@orderid", SqlDbType.Int).Value = order.IdOrder;
                    await cmd.ExecuteNonQueryAsync();

                    cmd.Parameters.Clear();
                    cmd.CommandText = "INSERT INTO Product_Warehouse(IdWarehouse,IdProduct,IdOrder,Amount,Price,CreatedAt) " +
                        "VALUES(@idwarehouse,@idproduct,@idorder,@amount,@price,@createdat)";
                    cmd.Parameters.AddWithValue("@idwarehouse", SqlDbType.Int).Value = productRegistration.IdWarehouse;
                    cmd.Parameters.AddWithValue("@idproduct", SqlDbType.Int).Value = productRegistration.IdProduct;
                    cmd.Parameters.AddWithValue("@idorder", SqlDbType.Int).Value = order.IdOrder;
                    cmd.Parameters.AddWithValue("@amount", SqlDbType.Int).Value = productRegistration.Amount;
                    cmd.Parameters.AddWithValue("@price", SqlDbType.Decimal).Value = prod.Price * productRegistration.Amount;
                    cmd.Parameters.AddWithValue("@createdat", SqlDbType.DateTime).Value = DateTime.Now;
                    await cmd.ExecuteNonQueryAsync();

                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT id=IDENT_CURRENT('Product_Warehouse')";
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            id = (int)(dr.GetDecimal("id"));
                        }
                    }
                    await tran.CommitAsync();

                }
                catch (SqlException e)
                {
                    await tran.RollbackAsync();
                    throw new Exception(e.Message);
                }
                catch (Exception e)
                {
                    await tran.RollbackAsync();
                    throw new Exception(e.Message);
                }
            }
            return id;
        }

        public async Task<int> RegisterProductByStoredProcedureAsync(ProductRegistration productRegistration)
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var com = new SqlCommand("AddProductToWarehouse", con);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@IdProduct", SqlDbType.Int).Value = productRegistration.IdProduct;
            com.Parameters.AddWithValue("@IdWarehouse", SqlDbType.Int).Value = productRegistration.IdWarehouse;
            com.Parameters.AddWithValue("@Amount", SqlDbType.Int).Value = productRegistration.Amount;
            com.Parameters.AddWithValue("@CreatedAt", SqlDbType.DateTime).Value = productRegistration.CreatedAt;

            await con.OpenAsync();
            int id = -1;
            using (var dr = await com.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    id = (int)(dr.GetDecimal("NewId"));
                }
            }

            return id;
        }
    }
}

