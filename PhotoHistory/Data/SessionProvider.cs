using System.Reflection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Mapping.ByCode;
using System.Linq;

namespace PhotoHistory.Data
{
    public class SessionProvider
    {
        private static ISessionFactory _sessionFactory;
        private static Configuration _config;

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    _sessionFactory = CreateSessionFactory();
                }
                return _sessionFactory;
            }
        }

        public static Configuration Config
        {
            get
            {
                if (_config == null)
                {
                    _config = new Configuration();

                    // To nizej robi mapowanie na podstawie wszystkich plikow *.hbm.xml w assembly:
                    //_config.AddAssembly( Assembly.GetCallingAssembly() );

                    ModelMapper modelMapper = new ModelMapper();
                    System.Type[] mappedTypes = typeof(UserMapping).Assembly.GetExportedTypes().Where(t => t.Name.EndsWith("Mapping")).ToArray();
                    modelMapper.AddMappings(mappedTypes);

                    _config.Proxy(p => p.ProxyFactoryFactory<NHibernate.Bytecode.DefaultProxyFactoryFactory>());
                    _config.DataBaseIntegration(d =>
                    {
                        d.ConnectionString = "Server=localhost;Port=5432;Database=pastexplorer;User Id=postgres;Password=qwe;";
                        d.Dialect<NHibernate.Dialect.PostgreSQLDialect>();
                        d.Driver<NHibernate.Driver.NpgsqlDriver>();
                        d.LogSqlInConsole = true;
                        d.LogFormattedSql = true;
                    });
                    _config.AddMapping(modelMapper.CompileMappingForAllExplicitlyAddedEntities());
                }

                return _config;
            }
        }

        private static ISessionFactory CreateSessionFactory()
        {
            return Config.BuildSessionFactory();
        }

        public static void RebuildSchema()
        {
            var schema = new SchemaExport(Config);
            schema.Create(true, true);
        }
    }
}