namespace CentralFrontend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        // Enable static files and default files
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.Run();
    }
}
