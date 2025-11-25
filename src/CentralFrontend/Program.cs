namespace CentralFrontend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //Se definen los servicios
        builder.Services.AddControllers();

        var app = builder.Build();

        //Se configura el pipeline HTTP
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapControllers();

        app.Run();
    }
}
