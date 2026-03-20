using CW_hammer.Data;
using CW_hammer.Pages;
using CW_hammer.Themes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace CW_hammer
{
    public partial class App : Application
    {
        private IHost _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeManager.Apply(AppTheme.Default);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    var connectionString =
                        context.Configuration.GetConnectionString("DefaultConnection");

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    services.AddTransient<BrowserWindow>();
                    services.AddTransient<HomePage>();
                    services.AddTransient<PatientsPage>();
                    services.AddTransient<StatisticsPage>();
                })
                .Build();

            _host.Start();

            var browser = _host.Services.GetRequiredService<BrowserWindow>();
            browser.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}