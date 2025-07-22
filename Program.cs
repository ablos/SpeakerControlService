using SpeakerControlService;
using System.Diagnostics;

try
{
    if (args.Length > 0)
    {
        switch (args[0].ToLower())
        {
            case "--install":
                InstallService();
                return;
            case "--uninstall":
                UninstallService();
                return;
            case "--help":
            case "-h":
                ShowHelp();
                return;
            default:
                Console.WriteLine($"Unknown argument: {args[0]}");
                ShowHelp();
                return;
        }
    }

    Console.WriteLine("Initializing Speaker Control Service...");
    var builder = Host.CreateApplicationBuilder(args);
    builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);
    builder.Services.Configure<Config>(builder.Configuration);
    builder.Services.AddHostedService<Worker>();
    builder.Services.AddHttpClient();
    builder.Services.AddSingleton<HomeAssistantService>();
    builder.Services.AddWindowsService();
    var host = builder.Build();
    Console.WriteLine("Initiated successfully. Starting...");
    host.Run();
}
catch (FileNotFoundException ex) when (ex.Message.Contains("config.json"))
{
    Console.WriteLine("ERROR: config.json file not found!");
    Console.WriteLine($"Expected location: {Path.Combine(Environment.CurrentDirectory, "config.json")}");
    Console.WriteLine("Make sure the config.json file is in the same directory as the executable.");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR: {ex.Message}");
    Console.WriteLine($"Type: {ex.GetType().Name}");
    if (ex.InnerException != null)
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    Environment.Exit(1);
}

static void InstallService()
{
    if (!IsRunningAsAdministrator())
    {
        Console.WriteLine("Requesting administrator privileges...");
        RestartAsAdministrator("--install");
        return;
    }

    try
    {
        Console.WriteLine("Installing Speaker Control Service...");

        var exePath = Environment.ProcessPath;
        var serviceName = "SpeakerControlService";

        if (!File.Exists(exePath))
        {
            Console.WriteLine($"ERROR: Executable not found at {exePath}");
            Environment.Exit(1);
        }

        // Create the service
        var arguments = $"create {serviceName} binPath= \"{exePath}\"";
        var startInfo = new ProcessStartInfo
        {
            FileName = "sc",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo);
        if (process != null)
        {
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("Service created successfully!");

                // Configure the service
                Console.WriteLine("Configuring service...");
                SetServiceDisplayName(serviceName, "Speaker Control Service");
                SetServiceDescription(serviceName, "Automatically controls speakers based on audio activity");
                SetServiceStartup(serviceName, "auto");

                // Start the service
                Console.WriteLine("Starting service...");
                var startServiceInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"start {serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var startProcess = Process.Start(startServiceInfo);
                if (startProcess != null)
                {
                    startProcess.WaitForExit();
                    if (startProcess.ExitCode == 0)
                    {
                        Console.WriteLine("Service installed and started successfully!");
                        Console.WriteLine("Your speakers will now be controlled automatically based on audio activity.");
                    }
                    else
                    {
                        Console.WriteLine("Service installed but failed to start.");
                        Console.WriteLine("You can start it manually from services.msc");
                    }
                }
            }
            else
            {
                Console.WriteLine("Failed to create service!");
                Console.WriteLine($"Exit code: {process.ExitCode}");
                Console.WriteLine($"Output: {output}");
                if (!string.IsNullOrEmpty(error))
                    Console.WriteLine($"Error: {error}");
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to install service: {ex.Message}");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        Environment.Exit(1);
    }
}

static void SetServiceDisplayName(string serviceName, string displayName)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "sc",
        Arguments = $"config {serviceName} DisplayName= \"{displayName}\"",
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(startInfo);
    process?.WaitForExit();
}

static void SetServiceDescription(string serviceName, string description)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "sc",
        Arguments = $"description {serviceName} \"{description}\"",
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(startInfo);
    process?.WaitForExit();
}

static void SetServiceStartup(string serviceName, string startType)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "sc",
        Arguments = $"config {serviceName} start= {startType}",
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(startInfo);
    process?.WaitForExit();
}

static void UninstallService()
{
    if (!IsRunningAsAdministrator())
    {
        Console.WriteLine("Requesting administrator privileges...");
        RestartAsAdministrator("--uninstall");
        return;
    }

    try
    {
        Console.WriteLine("Uninstalling Speaker Control Service...");

        var serviceName = "SpeakerControlService";

        // Stop service first
        var stopInfo = new ProcessStartInfo
        {
            FileName = "sc",
            Arguments = $"stop \"{serviceName}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var stopProcess = Process.Start(stopInfo))
        {
            stopProcess?.WaitForExit();
        }

        // Delete service
        var startInfo = new ProcessStartInfo
        {
            FileName = "sc",
            Arguments = $"delete \"{serviceName}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo);
        if (process != null)
        {
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"Service '{serviceName}' uninstalled successfully!");
            }
            else
            {
                Console.WriteLine("Failed to uninstall service!");
                Console.WriteLine($"Output: {output}");
                if (!string.IsNullOrEmpty(error))
                    Console.WriteLine($"Error: {error}");
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to uninstall service: {ex.Message}");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        Environment.Exit(1);
    }
}

static void RestartAsAdministrator(string arguments)
{
    try
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = Environment.ProcessPath,
            Arguments = arguments,
            UseShellExecute = true,
            Verb = "runas" // This triggers the UAC prompt
        };

        Process.Start(processInfo);
    }
    catch (System.ComponentModel.Win32Exception)
    {
        Console.WriteLine("User cancelled the administrator request.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to restart as administrator: {ex.Message}");
    }
}

static bool IsRunningAsAdministrator()
{
    try
    {
        var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }
    catch
    {
        return false;
    }
}

static void ShowHelp()
{
    Console.WriteLine("Speaker Control Service");
    Console.WriteLine("Usage:");
    Console.WriteLine("  SpeakerControlService.exe                 Run as console application");
    Console.WriteLine("  SpeakerControlService.exe --install       Install as Windows Service (auto-elevates)");
    Console.WriteLine("  SpeakerControlService.exe --uninstall     Uninstall Windows Service (auto-elevates)");
    Console.WriteLine("  SpeakerControlService.exe --help          Show this help message");
}