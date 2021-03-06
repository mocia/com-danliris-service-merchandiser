﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;
using TestHelpers = Com.Danliris.Service.Merchandiser.Test.Helpers;
using Com.Danliris.Service.Merchandiser.Lib.Helpers;
using Com.Danliris.Service.Merchandiser.Lib;
using Com.Danliris.Service.Merchandiser.Lib.Services;
using Com.Danliris.Service.Merchandiser.Test.DataUtils.LineDataUtil;
using Com.Danliris.Service.Merchandiser.Test.DataUtils.RateDataUtil;
using Com.Danliris.Service.Merchandiser.Test.DataUtils.EfficiencyDataUtil;


namespace Com.Danliris.Service.Merchandiser.Test
{
    public class ServiceProviderFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public void RegisterEndpoint(IConfigurationRoot Configuration)
        {
            APIEndpoint.Core = Configuration.GetValue<string>("CoreEndpoint") ?? Configuration["CoreEndpoint"];
            APIEndpoint.Inventory = Configuration.GetValue<string>("InventoryEndpoint") ?? Configuration["InventoryEndpoint"];
            APIEndpoint.Production = Configuration.GetValue<string>("ProductionEndpoint") ?? Configuration["ProductionEndpoint"];
            APIEndpoint.Purchasing = Configuration.GetValue<string>("PurchasingEndpoint") ?? Configuration["PurchasingEndpoint"];
        }

        public ServiceProviderFixture()
        {
            /*
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile("appsettings.json")
                .Build();
            */

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                {
                    /*
                    new KeyValuePair<string, string>("Authority", "http://localhost:5000"),
                    new KeyValuePair<string, string>("ClientId", "dl-test"),
                    */
                    new KeyValuePair<string, string>("Secret", "DANLIRISTESTENVIRONMENT"),
                    new KeyValuePair<string, string>("ASPNETCORE_ENVIRONMENT", "Test"),
                    new KeyValuePair<string, string>("CoreEndpoint", "http://localhost:5001/v1/"),
                    new KeyValuePair<string, string>("InventoryEndpoint", "http://localhost:5002/v1/"),
                    new KeyValuePair<string, string>("ProductionEndpoint", "http://localhost:5003/v1/"),
                    new KeyValuePair<string, string>("PurchasingEndpoint", "http://localhost:5004/v1/"),
                    new KeyValuePair<string, string>("DefaultConnection", "Server=localhost,1401;Database=com.danliris.db.merchandiser.service.test;User=sa;password=Standar123.;MultipleActiveResultSets=true;")
                })
                .Build();

            RegisterEndpoint(configuration);
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["DefaultConnection"];

            this.ServiceProvider = new ServiceCollection()
                .AddDbContext<MerchandiserDbContext>((serviceProvider, options) =>
                {
                    options.UseSqlServer(connectionString);
                }, ServiceLifetime.Transient)
                .AddTransient<LineService>(provider => new LineService(provider))
                .AddTransient<RateService>(provider => new RateService(provider))
                .AddTransient<LineDataUtil>()
                .AddTransient<RateDataUtil>()
                .AddTransient<EfficiencyService>(provider => new EfficiencyService(provider))
                .AddTransient<EfficiencyDataUtil>()

                .BuildServiceProvider();

            MerchandiserDbContext dbContext = ServiceProvider.GetService<MerchandiserDbContext>();
            dbContext.Database.Migrate();
        }

        public void Dispose()
        {
        }
    }

    [CollectionDefinition("ServiceProviderFixture Collection")]
    public class ServiceProviderFixtureCollection : ICollectionFixture<ServiceProviderFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}