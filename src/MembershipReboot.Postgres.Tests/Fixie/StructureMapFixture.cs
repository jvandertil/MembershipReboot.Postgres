using System.Configuration;
using BrockAllen.MembershipReboot;
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
        });

        public IContainer Container { get; }

        public StructureMapFixture()
        {
            Container = Root.CreateChildContainer();
        }
    }
}