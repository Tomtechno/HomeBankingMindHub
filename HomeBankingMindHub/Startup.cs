using HomeBankingMindHub.Controllers;
using HomeBankingMindHub.Models;
using HomeBankingMindHub.Repositories;
using HomeBankingMindHub.Repositories.Interfaces;
using HomeBankingMinHub.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeBankingMindHub
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.

        //Inyectamos las dependencias (AddRazorPages, AddControllers, AddDbContext, AddScoped)
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages(); //Permite utilizar páginas Razor (C#+html) en la aplicación (vistas)
            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve); //Se agregan los controladores y permite que los controladores respondan a las peticiones http
            services.AddDbContext<HomeBankingContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("HomeBankingConnection"))); // Agregamos el contexto
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ICardRepository, CardRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IClientLoanRepository, ClientLoanRepository>();
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<AccountsController>();
            services.AddScoped<AuthController>();
            services.AddScoped<CardsController>();
            services.AddScoped<ClientsController>();
            services.AddScoped<TransactionsController>();
            services.AddScoped<LoansController>();
            //autenticación, cuando el navegador envía una petición para acceder a algún recurso protegido el servidor web
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // It defines the process and rules for authentication
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10); //Tiempo de expiracion de la cookie
                options.LoginPath = new PathString("/index.html"); //Ruta de redirección en el caso de que se le cierre la sesion a un usuario
            });
            //autorización, las reglas que indican qué puede hacer el usuario o con qué recursos puede interactuar (permiso)
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ClientOnly", policy => policy.RequireClaim("Client")); // Se indica que se demanda un Client, cuando se le hace una peticion al back, el back nos demandarpa tener Client
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles(); //Cuando se hace una request, responde utilizando los contenidos de la carpeta wwwroot
            app.UseRouting(); //Determina que endpoint debe responder a la request
            app.UseAuthentication();
            app.UseAuthorization(); //Restrinje el acceso de acuerdo a los roles de cada usuario y la autentificacion
            app.UseEndpoints(endpoints => // Define las rutas finales que seran utilizadas para manejar las solicitudes entrantes
            {
                // Los endpoint refieren a URLs que por el cual el usuario hace una peticion y asi puede interactuar con la web
                endpoints.MapRazorPages(); //Para cada endpoint mapea el razorpages, tecnologia de ASP para construir paginas web utilizando c#
                endpoints.MapControllers(); //Establece una ruta para manejar las solicitudes a controladores quientes responden a la solicitudes http, permite usar los controladores
                //endpoints.MapControllerRoute( Used for configuring routes for traditional MVC controllers while MapControllers is for Web API controllers
                //    name: "default",
                //    pattern: "{controller=games}/{ action = Get}");

            });
        }
    }
}