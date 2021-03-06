﻿using System;
using Account.API.Application.IntegrationEvents.EventHandling;
using Account.API.Application.IntegrationEvents.Events;
using Account.API.Application.Queries;
using Account.API.Infrastructure;
using Account.API.Infrastructure.Filters;
using Account.API.Models;
using Account.API.Services;
using Account.API.Services.Email;
using Account.API.Services.Email.Extensions;
using AutoMapper;
using HealthChecks.UI.Client;
using Helpers.Extensions;
using Helpers.Mapping;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SecureChat.Common.Events.EventBus.Abstractions;
using SecureChat.Common.Events.EventBusRabbitMQ.Extensions;

namespace Account.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                //var policy = new AuthorizationPolicyBuilder()
                //    .RequireAuthenticatedUser()
                //    .Build();
                //options.Filters.Add(new AuthorizeFilter(policy));
                options.Filters.Add(typeof(GlobalExceptionFilter));
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddIdentity<User, IdentityRole>(config =>
                {
                    config.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<AccountDbContext>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddDefaultTokenProviders();

            services.AddDbContext<AccountDbContext>(options =>
            {
                options.UseMySql(Configuration["ConnectionString"],
                    opt =>
                    {
                        opt.MigrationsAssembly(typeof(Startup).Assembly.FullName);
                        opt.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: Array.Empty<int>());
                    });
            });

            services.AddAuthenticationWithBypass(opt =>
            {
                opt.Authority = Configuration["AuthUrl"];
                opt.Audience = "account";
                opt.RequireHttpsMetadata = false;
            }, opt =>
            {
                opt.BypassAuthenticationHeader = Configuration["BypassAuthenticationHeader"];
                opt.BypassAuthenticationSecret = Configuration["BypassAuthenticationSecret"];
            });

            // No need for origin restrictions, since the microservice will not be exposed externally
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.Configure<DbConnectionInfo>(Configuration);

            services.AddEventBus(options =>
            {
                options.HostName = Configuration["EventBusConnection"];
                options.UserName = Configuration["EventBusUserName"];
                options.Password = Configuration["EventBusPassword"];
                options.QueueName = Configuration["EventBusQueueName"];
                options.RetryCount = 10;
            }, typeof(Startup).Assembly);

            services.AddScoped<AccountDbContextSeed>();
            services.AddScoped<RolePermissionsService>();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IIdentityService, IdentityService>();

            services.AddHealthChecks()
                .AddCheck("self-check", () => HealthCheckResult.Healthy(), tags: new[] { "self" })
                .AddMySql(Configuration["ConnectionString"], name: "db-check", tags: new[] { "db" })
                .AddRabbitMQ($"amqp://{Configuration["EventBusUserName"]}:{Configuration["EventBusPassword"]}@{Configuration["EventBusConnection"]}", 
                    name: "rabbitmq-check", tags: new string[] { "rabbitmq" });

            services.AddAutoMapper(config =>
                {
                    config.AddProfile(new AutoMapperConfig(new[] {typeof(Startup).Assembly}));
                }
                ,typeof(Startup).Assembly);

            services.AddEmailSender(Configuration, HostingEnvironment);

            services.AddTransient<IEmailGenerator, DefaultEmailGenerator>();

            services.AddMediatR(typeof(Startup).Assembly);

            services.AddScoped<IUserQueries, UserQueries>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseCors();

            app.UseAuthentication();

            app.UseMvc();

            app.ConfigureEventBus();
        }
    }

    internal static class CustomExtensionMethods
    {
        public static IApplicationBuilder ConfigureEventBus(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<UserAccountUpdatedIntegrationEvent, UserAccountUpdatedIntegrationEventHandler>();

            return app;
        }
    }
}
