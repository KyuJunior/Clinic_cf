using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MedicalApp.Data;
using MedicalApp.Services;
using MedicalApp.ViewModels;
using MedicalApp.Views;

namespace MedicalApp
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;
        public IConfiguration Configuration { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set up Configuration builder to read appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Run database migrations on startup to ensure central DB is ready
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                using var dbContext = await factory.CreateDbContextAsync();
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect or migrate database on the local server:\n{ex.Message}", 
                                "Database Connection Error", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // Resolve and show MainWindow
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register Configuration
            services.AddSingleton<IConfiguration>(Configuration);

            // Register AppDbContext with DbContextFactory for WPF concurrency safety
            var connectionString = Configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
            
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register Services
            services.AddSingleton<ISharedStateService, SharedStateService>();
            services.AddTransient<IPatientService, PatientService>();
            services.AddTransient<IVisitService, VisitService>();
            services.AddTransient<IEchoService, EchoService>();

            // Register ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<PatientRegistrationViewModel>();
            services.AddTransient<ClinicalExamViewModel>();
            services.AddTransient<EchoUploadViewModel>();

            // Register Main Window
            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });
        }
    }
}
