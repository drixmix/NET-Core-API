﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API_Jwt_Auth.Data;
using API_Jwt_Auth.Data.Entity;
using API_Jwt_Auth.Data.Interfaces;
using API_Jwt_Auth.Data.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API_Jwt_Auth {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            services.AddIdentity<User, IdentityRole>(cfg => {

                cfg.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<Db_Context>();

            services.AddAuthentication()
                    .AddCookie()
                    .AddJwtBearer(cfg => {

                        cfg.TokenValidationParameters = new TokenValidationParameters() {
                            ValidIssuer = Configuration["Tokens:Issuer"],
                            ValidAudience = Configuration["Tokens:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
                        };


                        //cfg.Events = new JwtBearerEvents {
                        //    OnMessageReceived = context =>
                        //    {
                        //        var accessToken = context.Request.Query["access_token"];

                        //        // If the request is for our hub...
                        //        var path = context.HttpContext.Request.Path;
                        //        if (!string.IsNullOrEmpty(accessToken) &&
                        //            (path.StartsWithSegments("/chat"))) {
                        //            // Read the token out of the query string
                        //            context.Token = accessToken;
                        //        }
                        //        return Task.CompletedTask;
                        //    }
                        //};


                    });

            services.AddDbContext<Db_Context>(cfg => {
                cfg.UseSqlServer(Configuration.GetConnectionString("MyConnectionString"));
            });
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddTransient<SeederData>();
            services.AddMvc()
                .AddJsonOptions(opt => opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseSignalR(routes => {
                routes.MapHub<ChatHub>("/chat");
            });

            app.UseMvc(cfg => {
                cfg.MapRoute("Default",
                    "{controller}/{action}/{id?}"
                    );
            });
        }
    }
}
