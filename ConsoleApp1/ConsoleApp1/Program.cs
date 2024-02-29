using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Management;

class Program
{
    static void Main(string[] args)
    {
        // Dados do computador
        string hostName = Environment.MachineName;
        string userName = Environment.UserName;
        string ipAddress = GetIPAddress();
        string macAddress = GetMACAddress();
        string totalMemory = GetTotalMemory();
        string totalStorage = GetTotalStorage();
        string processorModel = GetProcessorModel();
        string operatingSystem = GetOperatingSystem();
        string computerBrand = GetComputerBrand();
        string computerModel = GetComputerModel();

        string databasePath = "computers.db";
        ComputerPersistence computerPersistence = new ComputerPersistence(databasePath);

        // Dados do computador
        Dictionary<string, object> computerData = new Dictionary<string, object>
        {
            { "HostName", hostName },
            { "UserName", userName },
            { "IPAddress",ipAddress },
            { "MACAddress", macAddress },
            { "TotalMemory", totalMemory},
            { "TotalStorage", totalStorage },
            { "ProcessorModel", processorModel },
            { "OperatingSystem", operatingSystem },
            { "ComputerBrand", computerBrand },
            { "ComputerModel", computerModel }
        };
              // Inserir os dados do computador
        computerPersistence.InsertComputerData(computerData);

        Console.WriteLine("Dados do computador persistidos com sucesso!");

        // Consultar os dados do computador
        List<Dictionary<string, object>> results = computerPersistence.GetAllComputerData();

        // Exibir os resultados
        foreach (Dictionary<string, object> result in results)
        {
            Console.WriteLine("Dados do computador:");
            foreach (KeyValuePair<string, object> pair in result)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
            }
        }

        Console.ReadKey();
    }

    // Métodos auxiliares para obter os dados do computador

    static string GetIPAddress()
    {
        string ipAddress = "";
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT IPAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'"))
        {
            ManagementObjectCollection managementObjects = searcher.Get();
            foreach (ManagementObject managementObject in managementObjects)
            {
                string[] addresses = (string[])managementObject["IPAddress"];
                ipAddress = addresses[0];
                break;
            }
        }

        return ipAddress;
    }

    static string GetMACAddress()
    {
        string macAddress = GetManagementObjectProperty("Win32_NetworkAdapter", "MACAddress", "NetConnectionID != NULL");
        return macAddress;
    }

    static string GetTotalMemory()
    {
        string totalMemory = GetManagementObjectProperty("Win32_ComputerSystem", "TotalPhysicalMemory", null);
        ulong memorySize = Convert.ToUInt64(totalMemory);
        double totalMemoryGB = memorySize / (1024.0 * 1024.0 * 1024.0);
        return totalMemoryGB.ToString("N2") + " GB";
    }

    static string GetTotalStorage()
    {
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Size FROM Win32_LogicalDisk WHERE DriveType = 3"))
        {
            ManagementObjectCollection managementObjects = searcher.Get();
            double totalSize = 0;
            foreach (ManagementObject managementObject in managementObjects)
            {
                double size = Convert.ToDouble(managementObject["Size"]);
                totalSize += size;
            }

            double totalStorage = totalSize / 1024 / 1024 / 1024; // Convertendo para GB
            return totalStorage.ToString("N2") + " GB";
        }
    }

    static string GetProcessorModel()
    {
        string processorModel = GetManagementObjectProperty("Win32_Processor", "Name", null);
        return processorModel;
    }

    static string GetOperatingSystem()
    {
        string operatingSystem = GetManagementObjectProperty("Win32_OperatingSystem", "Caption", null);
        return operatingSystem;
    }

    static string GetComputerBrand()
    {
        string computerBrand = GetManagementObjectProperty("Win32_ComputerSystem", "Manufacturer", null);
        return computerBrand;
    }

    static string GetComputerModel()
    {
        string computerModel = GetManagementObjectProperty("Win32_ComputerSystem", "Model", null);
        return computerModel;
    }

    static string GetManagementObjectProperty(string className, string propertyName, string condition)
    {
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className} {(condition != null ? "WHERE " + condition : "")}"))
        {
            ManagementObjectCollection managementObjects = searcher.Get();
            foreach (ManagementObject managementObject in managementObjects)
            {
                string propertyValue = managementObject[propertyName]?.ToString();
                if (!string.IsNullOrEmpty(propertyValue))
                {
                    return propertyValue;
                }
            }
        }

        return string.Empty;
    }
}