using Autofac;
using AutoMapper;
using Business.Abstract;
using Business.Concrete;
using Business.DependencyRepository.AutoMapper;
using Core.Utilities.Security.JWT;
using DataAccess.Abstract;
using DataAccess.Concrete.Dapper;
using DataAccess.Concrete.Dapper.Context;
using DataAccess.Concrete.EntityFramework;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Net.Http;

namespace Business.DependencyRepository.Autofac
{
    public class AutofacBusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            #region Dapper

            builder.Register(c =>
            {
                var connectionString = ContextDb.ConnectionStringDefault;
                return new SqlConnection(connectionString);
            }).As<IDbConnection>().InstancePerLifetimeScope();

            builder.RegisterType<LogErrorRepository>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<GenericRepository>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<MiddlewareRepository>().AsSelf().InstancePerLifetimeScope();

            #endregion Dapper

            builder.RegisterType<Enigma.Processor>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<JwtHelper>().As<ITokenHelper>();
            builder.RegisterType<CacheService>().As<ICacheService>();

            builder.RegisterType<EfErrorLogDal>().As<IErrorLogDal>();
            builder.RegisterType<ErrorLogManager>().As<IErrorLogService>();

            builder.RegisterType<EfUserActivityLogDal>().As<IUserActivityLogDal>();
            builder.RegisterType<EfUserDal>().As<IUserDal>().InstancePerLifetimeScope();

            builder.RegisterType<AccountManager>().As<IAccountService>();
            builder.RegisterType<EfRoleDal>().As<IRoleDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfUserRoleDal>().As<IUserRoleDal>().InstancePerLifetimeScope();

            builder.RegisterType<ChatManager>().As<IChatService>();

            builder.RegisterType<EfProductDal>().As<IProductDal>();
            builder.RegisterType<ProductManager>().As<IProductService>();

            builder.RegisterType<EfCategoryDal>().As<ICategoryDal>();
            builder.RegisterType<CategoryManager>().As<ICategoryService>();

            builder.RegisterType<EfCustomerDal>().As<ICustomerDal>();
            builder.RegisterType<CustomerManager>().As<ICustomerService>();

            builder.Register(c =>
            {

                var httpClientFactory = c.Resolve<IHttpClientFactory>();

                var configuration = c.Resolve<IConfiguration>();
                var apiKey = configuration["Gemini:ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Gemini API anahtarý (Gemini:ApiKey) appsettings.json dosyasýnda bulunamadý.");
                }

                // Sýnýf adýnýzýn GeminiManager olduðundan emin olun
                return new GeminiManager(httpClientFactory.CreateClient(), apiKey);

            }).As<IGeminiService>().SingleInstance();


            builder.Register(context => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Maps>();
            }
            )).AsSelf().SingleInstance();

            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                var config = context.Resolve<MapperConfiguration>();
                return config.CreateMapper(context.Resolve);
            })
            .As<IMapper>()
            .InstancePerLifetimeScope();
        }
    }
}