using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Hierarchical;

namespace MembershipReboot.Postgres.Tests
{
    public class UserAccountServiceIntegrationTests
    {
        public class TheCreateMethod
        {
            public void Test(UserAccountService<HierarchicalUserAccount> service, string un)
            {
                service.CreateAccount("Test", "Testpass", "test@test.nl");
            }
        }
    }
}