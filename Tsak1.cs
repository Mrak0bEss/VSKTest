[WebMethod]
public Order LoadOrderInfo(string orderCode)
{
    if (string.IsNullOrEmpty(orderCode))
        throw new ArgumentException("orderCode cannot be null or empty", nameof(orderCode));

    Stopwatch stopWatch = new Stopwatch();
    stopWatch.Start();

    try
    {
        lock (cache)
        {
            if (cache.ContainsKey(orderCode))
            {
                stopWatch.Stop();
                logger.Log("INFO", "Elapsed - {0}", stopWatch.Elapsed);

                return cache[orderCode];
            }
        }

        string query = "SELECT OrderID, CustomerID, TotalMoney FROM dbo.Orders where OrderCode=@orderCode";
        using (SqlConnection connection = new SqlConnection(this.connectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@orderCode", orderCode);
            connection.Open();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Order order = new Order((string)reader[0], (string)reader[1], (int)reader[2]);

                    lock (cache)
                    {
                        if (!cache.ContainsKey(orderCode))
                            cache[orderCode] = order;
                    }
                    stopWatch.Stop();
                    logger.Log("INFO", "Elapsed - {0}", stopWatch.Elapsed);

                    return order;
                }
            }
        }

        stopWatch.Stop();
        logger.Log("INFO", "Elapsed - {0}", stopWatch.Elapsed);

        return null;
    }
    catch (Exception ex)
    {
        logger.Log("ERROR", ex.Message);
        throw;
    }
}