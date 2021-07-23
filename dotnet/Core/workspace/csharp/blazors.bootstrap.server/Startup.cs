namespace Blazors.Bootstrap.Server
{
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Domain.Derivations.Rules.Default;
    using Allors.Database.Meta;
    using Allors.Security;
    using Allors.Services;
    using Blazors.Bootstrap.Server.Areas.Identity;
    using BlazorStrap;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using ObjectFactory = Allors.Database.ObjectFactory;
    using User = Allors.Database.Meta.User;

    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.Configuration = configuration;
            this.Environment = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBootstrapCss();

            // Allors
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IPolicyService, PolicyService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();

            services.AddDefaultIdentity<IdentityUser>()
                .AddAllorsStores();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            // Allors
            var metaPopulation = new MetaBuilder().Build();
            var engine = new Engine(Rules.Create(metaPopulation));
            var objectFactory = new ObjectFactory(metaPopulation, typeof(Allors.Database.Domain.User));
            var databaseScope = new DefaultDomainDatabaseServices(engine, httpContextAccessor);
            var databaseBuilder = new DatabaseBuilder(databaseScope, this.Configuration, objectFactory);
            app.ApplicationServices.GetRequiredService<IDatabaseService>().Database = databaseBuilder.Build();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
