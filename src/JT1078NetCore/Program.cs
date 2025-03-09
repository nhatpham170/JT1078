using JT1078NetCore;
using JT1078NetCore.Socket;
using System.Threading.Tasks;


namespace JT1078NetCore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            

            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel((context, serverOptions) =>
            {
                var kestrelSection = context.Configuration.GetSection("Kestrel");

                serverOptions.Configure(kestrelSection)
                    .Endpoint("HTTP", listenOptions =>
                    {
                        
                    });
            });
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHostedService<SocketService>();
            var app = builder.Build();
         
            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();            
            app.MapControllers();            
            //await Host.CreateDefaultBuilder(args)
            //   .ConfigureWebHostDefaults(webBuilder =>
            //   {
            //       webBuilder.UseStartup<Startup>();
            //   }).Build().RunAsync();
            
            app.Run();
            
            //CreateHostBuilder(args).Build().Run();
        }       

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
           

    }
}


