using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

public class ComputerPersistence
{
    private string connectionString;

    public ComputerPersistence(string databasePath)
    {
        connectionString = $"Data Source={databasePath};Version=3;";
        InitializeTable();
    }

    private void InitializeTable()
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Computers (
                    HostName TEXT UNIQUE,
                    UserName TEXT,
                    IPAddress TEXT,
                    MACAddress TEXT,
                    TotalMemory TEXT,
                    TotalStorage TEXT,
                    ProcessorModel TEXT,
                    OperatingSystem TEXT,
                    ComputerBrand TEXT,
                    ComputerModel TEXT
                )";

            using (var command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    public void InsertComputerData(Dictionary<string, object> data)
    {
        var connection = new SQLiteConnection(connectionString);
        try
		{
          
        //using (var connection = new SQLiteConnection(connectionString))
        
            connection.Open();

				var insertQuery = @"
                INSERT INTO Computers (HostName, UserName, IPAddress, MACAddress, TotalMemory, TotalStorage, ProcessorModel, OperatingSystem, ComputerBrand, ComputerModel)
                VALUES (@HostName, @UserName, @IPAddress, @MACAddress, @TotalMemory, @TotalStorage, @ProcessorModel, @OperatingSystem, @ComputerBrand, @ComputerModel)";

				using (var command = new SQLiteCommand(insertQuery, connection))
            {
                foreach (var pair in data)
                {
                    command.Parameters.AddWithValue("@" + pair.Key, pair.Value);
                }

                command.ExecuteNonQuery();
            }
        }
		
		catch (SQLiteException ex)
        {

            // Log do erro em um arquivo
            string logFilePath = "error.log";
            string errorMessage = $"Erro ao inserir dados do computador: {ex.Message} ";
            string dataString = ConvertDataToString(data);


            WriteLogToFile(logFilePath, errorMessage, dataString);
        }
        finally
        {
            // Garanta que a conexão seja fechada, independentemente de ocorrer uma exceção ou não
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }



    }
    private void WriteLogToFile(string filePath, string message, string sqlquery)
    {
        try
        {
            using (var writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message} - query: \n{sqlquery}");
            }
        }
        catch (Exception ex)
        {
            // Trate qualquer erro que ocorra ao escrever no arquivo de log
            Console.WriteLine($"Erro ao escrever no arquivo de log: {ex.Message}");
        }
    }
    private string ConvertDataToString(Dictionary<string, object> data)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var entry in data)
        {
            sb.AppendLine($"{entry.Key}: {entry.Value}");
        }

        return sb.ToString();
    }

    public List<Dictionary<string, object>> GetAllComputerData()
    {
        var results = new List<Dictionary<string, object>>();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            var selectQuery = "SELECT * FROM Computers";

            using (var command = new SQLiteCommand(selectQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rowData = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            rowData[reader.GetName(i)] = reader.GetValue(i);
                        }

                        results.Add(rowData);
                    }
                }
            }
        }

        return results;
    }
}