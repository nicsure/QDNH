

using QDNH;
using QDNH.Settings;
using System.Globalization;

Console.WriteLine($"Quansheng Dock Network Host {Vars.Version}\n");
AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
Main.Run(args);

static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Exception exception = e.ExceptionObject as Exception ?? new("Unknown Exception Event");
    if (exception != null)
    {
        Console.Error.WriteLine($"An unhandled exception {exception} occurred:\n\n{exception.Message}");
    }
}