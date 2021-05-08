using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using ST.FetchingData.Infrastructure.Filters;

namespace ST.FetchingData
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //services.AddSwaggerGen(options =>
            //{
                
            //    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ST.FetchingData", Version = "v1" });
            //    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            //    {
            //        Type = SecuritySchemeType.OAuth2,
            //        Flows = new OpenApiOAuthFlows()
            //        {
            //            Implicit = new OpenApiOAuthFlow()
            //            {
            //                AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityEndpoint")}/oauth2/v2.0/authorize"),
            //                TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityEndpoint")}/oauth2/v2.0/token"),
            //                Scopes = new Dictionary<string, string>()
            //                {
            //                    { Configuration.GetValue<string>("IdentityAudience"), "FetchingData API" }
            //                }
            //            }
            //        }
            //    });

            //    options.OperationFilter<AuthorizeCheckOperationFilter>();
            //});

            ConfigureAuthService(services);

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddOptions();

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                options.Filters.Add(typeof(ValidateModelStateFilter));

            }).AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => {
                //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FetchingData v1");
                //    c.OAuthClientId("FetchingData");
                //    c.OAuthAppName("FetchingData Swagger UI");
                //    c.DefaultModelsExpandDepth(-1);
                //});
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("CorsPolicy");
            ConfigureAuth(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureAuthService(IServiceCollection services)
        {
            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

            var identityAuthority = Configuration.GetValue<string>("IdentityAuthority");
            var audience = Configuration.GetValue<string>("IdentityAudience");            

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                var events = new JwtBearerEvents();
                events.OnChallenge = (context) =>
                { return Task.FromResult(0); };
                events.OnAuthenticationFailed = (context) =>
                { return Task.FromResult(0); };
                events.OnForbidden = (context) =>
                { return Task.FromResult(0); };
                events.OnTokenValidated = (context) =>
                { return Task.FromResult(0); };
                events.OnMessageReceived = (context) =>
                { return Task.FromResult(0); };
                options.Events = events;

                options.Authority = identityAuthority;
                options.TokenValidationParameters.ValidAudience = audience;
            });

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //   .AddMicrosoftIdentityWebApi(options =>
            //   {
            //       Configuration.Bind("AzureAdB2C", options);

            //       options.TokenValidationParameters.NameClaimType = "name";
            //   },
            //   options => {
            //       Configuration.Bind("AzureAdB2C", options);
            //   });
        }

        protected virtual void ConfigureAuth(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
