using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;

namespace MembershipReboot.Postgres.Tests.Fixie
{
    public class StructureMapCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var contextFixture = new StructureMapFixture();

            fixture.Register(() => contextFixture);
            fixture.Customizations.Add(new ContainerBuilder(contextFixture.Container));
        }
    }

}
