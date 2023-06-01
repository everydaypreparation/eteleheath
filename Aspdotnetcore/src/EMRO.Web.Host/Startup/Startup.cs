using Abp.AspNetCore;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Castle.Logging.Log4Net;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Json;
using AspNetCoreRateLimit;
using Castle.Facilities.Logging;
using DinkToPdf;
using DinkToPdf.Contracts;
using EMRO.Common;
using EMRO.Common.CronJob;
using EMRO.Common.Paubox;
using EMRO.Common.Paubox.Dto;
using EMRO.Common.RealTimeNotification;
using EMRO.Common.Samvaad;
using EMRO.Common.Samvaad.Dto;
using EMRO.CommonSetting;
using EMRO.Configuration;
using EMRO.Email;
using EMRO.Identity;
using EMRO.Models;
using EMRO.Payment.Dto;
using EMRO.Versioning;
using EMRO.Web.Host.Services.SMS;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Stripe;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Clients;

namespace EMRO.Web.Host.Startup
{
    public class Startup
    {
        private const string _defaultCorsPolicyName = "localhost";

        // private const string _apiVersion = "v1";
        public IConfiguration Configuration { get; }
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _env;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _appConfiguration = env.GetAppConfiguration();
            Configuration = configuration;
            _env = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //Mailkit
            services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            services.Configure<UplodedFilePath>(Configuration.GetSection("UplodedFilePath"));
            services.Configure<DataEncryptionSetting>(Configuration.GetSection("ChiperInfo"));
            services.AddSingleton<IMailer, Mailer>();

            //Samvaad
            services.Configure<SamvaadParams>(Configuration.GetSection("SamvaadParams"));
            services.AddSingleton<IJoinMeeting, JoinMeeting>();
            services.AddScoped<ICronJobAppService, CronJobAppService>();

            //Paubox
            services.Configure<PauboxParams>(Configuration.GetSection("PauboxParams"));
            services.AddSingleton<IPaubox, Paubox>();

            //MVC
            services.AddControllersWithViews(
                options =>
                {
                    options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());


                }
            ).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new AbpMvcContractResolver(IocManager.Instance)
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
            });

            #region API Versioning  
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader =
                  new HeaderApiVersionReader("X-API-Version");
            });
            #endregion

            //services.AddDistributedMemoryCache();
            //services.AddDistributedPostgreSqlCache(setup =>
            //{
            //    setup.ConnectionString = Configuration.GetConnectionString(EMROConsts.ConnectionStringName);
            //    setup.SchemaName = "public";
            //    setup.TableName = "PgSession";
            //    setup.CreateInfrastructure = true;
            //});

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);


            services.AddSignalR().AddMessagePackProtocol();

            services.AddMvc();
            services.Configure<StripeSetting>(Configuration.GetSection("Stripe"));
            //Twilio Client
            services.AddHttpClient<ITwilioRestClient, CustomTwilioClient>();
            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddHttpContextAccessor();
            services.AddDataProtection();

            #region impersonation code

            services.Configure<SecurityStampValidatorOptions>(options => // different class name
            {
                options.ValidationInterval = TimeSpan.FromMinutes(1);  // new property name
                options.OnRefreshingPrincipal = x =>             // new property name
                {
                    var originalUserIdClaim = x.CurrentPrincipal.FindFirst("OriginalUserId");
                    var isImpersonatingClaim = x.CurrentPrincipal.FindFirst("IsImpersonating");
                    if (isImpersonatingClaim.Value == "true" && originalUserIdClaim != null)
                    {
                        x.NewPrincipal.Identities.First().AddClaim(originalUserIdClaim);
                        x.NewPrincipal.Identities.First().AddClaim(isImpersonatingClaim);
                    }
                    return Task.FromResult(0);
                };
            });

            #endregion

            try
            {
                services.AddHangfire(x => x.UsePostgreSqlStorage(_appConfiguration["ConnectionStrings:Default"]));
            }
            catch (Exception ex)
            {
                Console.WriteLine("error setup hangfire: " + ex.StackTrace.ToString());
                throw ex.InnerException;
            }

            // Configure CORS for angular2 UI
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .WithOrigins(
                            // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                            _appConfiguration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                )
            );


            services.AddSwaggerGen(options =>
            {
                // Swagger - Enable this line and the related lines in Configure method to enable swagger UI
                //options.SwaggerDoc(_apiVersion, new OpenApiInfo
                //{
                //    Version = _apiVersion,
                //    Title = "EMRO API",
                //    Description = "EMRO",
                //    // uncomment if needed TermsOfService = new Uri("https://example.com/terms"),
                //    Contact = new OpenApiContact
                //    {
                //        Name = "EMRO",
                //        Email = string.Empty,
                //        Url = new Uri("https://twitter.com/aspboilerplate"),
                //    },
                //    License = new OpenApiLicense
                //    {
                //        Name = "MIT License",
                //        Url = new Uri("https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/LICENSE"),
                //    }
                //});
                //options.DocInclusionPredicate((docName, description) => true);


                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = " ETeleHealth v1 API's",
                    Description = $"ETeleHealth API's for integration with UI \r\n\r\n © Copyright {DateTime.UtcNow.Year} ETeleHealth. All rights reserved."
                });
                options.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2",
                    Title = "ETeleHealth v2 API's",
                    Description = $"ETeleHealth API's for integration with UI \r\n\r\n © Copyright {DateTime.UtcNow.Year} ETeleHealth. All rights reserved."
                });
                options.ResolveConflictingActions(a => a.First());
                options.OperationFilter<RemoveVersionFromParameterv>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPath>();

                // Define the BearerAuth scheme that's in use
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });
            });

            var architectureFolder = (IntPtr.Size == 8) ? "64 bit" : "32 bit";
            var wkHtmlToPdfPath = Path.Combine(_env.ContentRootPath, "wkhtmltox", "v0.12.4", architectureFolder, "libwkhtmltox.so");

            CustomAssemblyLoadContext context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(wkHtmlToPdfPath);
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            // Configure Abp and Dependency Injection
            return services.AddAbp<EMROWebHostModule>(
                // Configure Log4Net logging
                options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig("log4net.config")
                )
            );
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            StripeConfiguration.SetApiKey(Configuration.GetSection("Stripe")["SecretKey"]);
#pragma warning restore CS0618 // Type or member is obsolete

            app.UseAbp(options => { options.UseAbpRequestLocalization = false; }); // Initializes ABP framework.

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ETeleHealth API v1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "ETeleHealth API v2");
            });

            app.UseCors(_defaultCorsPolicyName); // Enable CORS!

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAbpRequestLocalization();

            app.UseIpRateLimiting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<SignalRHub>("/signalr");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
            });

            //Hangfire service
            try
            {
                app.UseHangfireServer();
                app.UseHangfireDashboard();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in hangfire server and dashboard: " + ex.StackTrace.ToString());
                throw ex.InnerException;
            }
            try
            {
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.HourNotification(24), "*/30 * * * *");
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.HourTenNotification(10), "*/15 * * * *");
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.HourSixNotification(6), "*/15 * * * *");
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.HourOneNotification(1), "*/10 * * * *");
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.AppointmentNotification(), "*/2 * * * *");
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.UserAccountNotification(), "0 */23 * * *");
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.UserAccountNotificationEveryYear(), Cron.Daily);
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.SlotReminder(), "0 0 * * *");
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.UnusedPatient(), "0 0 * * *");
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.IsConsultantProfileComplete(), "0 0 * * SUN");
                RecurringJob.AddOrUpdate<CronJobAppService>(x => x.IsLegalProfileComplete(), "0 0 * * MON");
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in executing recurring job: " + ex.StackTrace.ToString()); throw ex.InnerException;
            }

            // // Enable middleware to serve generated Swagger as a JSON endpoint
            // app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });

            // // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            // app.UseSwaggerUI(options =>
            // {
            //      // specifying the Swagger JSON endpoint.
            //      options.SwaggerEndpoint($"/swagger/{_apiVersion}/swagger.json", $"EMRO API {_apiVersion}");

            //     options.IndexStream = () => Assembly.GetExecutingAssembly()
            //.GetManifestResourceStream("EMRO.Web.Host.wwwroot.swagger.ui.index.html");
            //     options.DisplayRequestDuration(); // Controls the display of the request duration (in milliseconds) for "Try it out" requests.  
            //  }); // URL: /swagger


        }
    }
}
