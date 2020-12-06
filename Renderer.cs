// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace RazorRenderer
{
    public class Renderer
    {
        public static IServiceScopeFactory ServiceScopeFactory { get; set; }

        public static void Initialize()
        {
            ServiceScopeFactory = InitializeServices();
        }

        public static Task<string> RenderViewAsync<TModel>(string viewName, TModel model) =>
            RenderViewAsync(ServiceScopeFactory, viewName, model);

        private static IServiceScopeFactory InitializeServices(string customApplicationBasePath = null)
        {
            // Initialize the necessary services
            var services = new ServiceCollection();
            ConfigureDefaultServices(services, customApplicationBasePath);

            // Add a custom service that is used in the view.
            services.AddSingleton<EmailReportGenerator>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        private static Task<string> RenderViewAsync<TModel>(IServiceScopeFactory scopeFactory, string viewName, TModel model)
        {
            IServiceScope serviceScope = scopeFactory.CreateScope();

            RazorViewToStringRenderer renderer = serviceScope.ServiceProvider
                .GetRequiredService<RazorViewToStringRenderer>();

            return renderer.RenderViewToStringAsync(viewName, model);
        }

        private static void ConfigureDefaultServices(IServiceCollection services, string customApplicationBasePath)
        {
            string applicationName;
            IFileProvider fileProvider;
            if (!string.IsNullOrEmpty(customApplicationBasePath))
            {
                applicationName = Path.GetFileName(customApplicationBasePath);
                fileProvider = new PhysicalFileProvider(customApplicationBasePath);
            }
            else
            {
                applicationName = Assembly.GetEntryAssembly().GetName().Name;
                fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            }

            services.AddSingleton<IWebHostEnvironment>(new WebHostEnvironment
            {
                ApplicationName =  applicationName,
                WebRootFileProvider = fileProvider,
            });

            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");

            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton<DiagnosticListener>(diagnosticSource);
            services.AddLogging();
            services.AddControllersWithViews();
            services.AddTransient<RazorViewToStringRenderer>();
        }
    }
}
