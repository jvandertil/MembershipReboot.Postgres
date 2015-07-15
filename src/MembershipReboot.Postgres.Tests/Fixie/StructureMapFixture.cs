using System.Configuration;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Hierarchical;
using Npgsql;
using StructureMap;

namespace MembershipReboot.Postgres.Tests.Fixie
{
    public class StructureMapFixture
    {
        public static IContainer Root = new Container(cfg =>
        {
            cfg.For<NpgsqlConnection>().Use("from_connection_string", ctx =>
            {
                var cs = ConfigurationManager.ConnectionStrings["TestDb"]
                                                 .ConnectionString;

                return new NpgsqlConnection(cs);
            }).Transient();

            cfg.For<IGroupRepository<HierarchicalGroup>>().Use<NpgsqlGroupRepository>();
            cfg.For<IUserAccountRepository<HierarchicalUserAccount>>().Use<NpgsqlUserAccountRepository>();
            cfg.For<MembershipRebootConfiguration<HierarchicalUserAccount>>().Use("test", ctx =>
            {
                var config = new MembershipRebootConfiguration<HierarchicalUserAccount>();
                
                // Low amount of iterations to speed up testing.
                config.PasswordHashingIterationCount = 100;

                // Accounts can be valid immediately in testing.
                config.RequireAccountVerification = false;

                return config;
            });
        });

        public IContainer Container { get; }

        public StructureMapFixture()
        {
            Container = Root.CreateChildContainer();
        }
    }
}