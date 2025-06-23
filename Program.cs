using Microsoft.EntityFrameworkCore;
using ST10092132.Data;
using Azure.Storage.Blobs; 

namespace ST10092132
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register DB Context
            builder.Services.AddDbContext<POEDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("LiveConn")));

            // Add Azure Blob Service Client (NEW)
            builder.Services.AddSingleton(x => new BlobServiceClient(
                builder.Configuration.GetValue<string>("AzureStorage:ConnectionString")
            ));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Ensure this is present for static assets
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
