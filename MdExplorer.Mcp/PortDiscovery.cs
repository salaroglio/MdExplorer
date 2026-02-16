namespace MdExplorer.Mcp;

public static class PortDiscovery
{
    public static int GetPort()
    {
        // Priority 1: Environment variable
        var envPort = Environment.GetEnvironmentVariable("MDEXPLORER_PORT");
        if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out var port))
            return port;

        // Priority 2: Port file in %AppData%/MdExplorer/port.txt
        var portFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MdExplorer",
            "port.txt");

        if (File.Exists(portFile))
        {
            var content = File.ReadAllText(portFile).Trim();
            if (int.TryParse(content, out var filePort))
                return filePort;
        }

        // Fallback: default port
        return 5000;
    }
}
