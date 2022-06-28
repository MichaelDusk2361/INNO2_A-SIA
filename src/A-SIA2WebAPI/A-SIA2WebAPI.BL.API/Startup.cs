using A_SIA2WebAPI.BL.API.Controllers;
using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.Tasks;
using A_SIA2WebAPI.BL.CalculationSystem;
using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.BL.PluginSystem;

namespace A_SIA2WebAPI.BL.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder => {
                        builder.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();
                    });
            });
            services.AddControllers();
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "A-SIA2 Web API", Version = "v0.1" });
                // Define swagger token auth
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        System.Array.Empty<string>()
                    }
                });
            });

            // Configure DAL
            // In future, configuration of connection is done here
            services.AddSingleton<Neo4JDatabase>();
            services.AddSingleton<INeo4JEngine, Neo4JEngine>();

            // Configure Repositories
            services.AddSingleton<IRepository<User>, Neo4JUserRepository>();
            services.AddSingleton<IRepository<Person>, Neo4JPersonRepository>();
            services.AddSingleton<IRepository<Group>, Neo4JGroupRepository>();
            services.AddSingleton<IRepository<Network>, Neo4JNetworkRepository>();
            services.AddSingleton<IRepository<Project>, Neo4JProjectRepository>();
            services.AddSingleton<IRepository<Instance>, Neo4JInstanceRepository>();
            services.AddSingleton<IRepository<Relation>, Neo4JRelationRepository>();
            services.AddSingleton<IRepository<NetworkStructure>, Neo4JNetworkStructureRepository>();

            // Configure password hashing service
            services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

            // Configure jwt service
#if DEBUG
            IdentityModelEventSource.ShowPII = true;
#endif
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.Configuration = new OpenIdConnectConfiguration();
#if DEBUG
                x.RequireHttpsMetadata = true;
                x.Authority = "https://localhost:5001";
#else
                x.RequireHttpsMetadata = true;
                x.Authority = "https://localhost:5001";
#endif
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(JwtAuthenticationService.TokenKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
                x.Audience = "api1";
            });

            // Add the jwt service for token auth
            services.AddSingleton<IJwtAuthenticationService, JwtAuthenticationService>();

            // Add calculation engine
            services.AddSingleton<IPluginLoader<ICalculator>, PluginLoader<ICalculator>>();
            services.AddSingleton<ICalculationEngine, CalculationEngine>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "A-SIA2 Web API v0.1"));
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
