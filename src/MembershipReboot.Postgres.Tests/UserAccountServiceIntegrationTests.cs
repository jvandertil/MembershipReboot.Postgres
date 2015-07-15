using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Hierarchical;
using Shouldly;

namespace MembershipReboot.Postgres.Tests
{
    public class UserAccountServiceIntegrationTests
    {
        public class TheCreateAccountMethod
        {
            public void CanCreateANewUserAccount(UserAccountService<HierarchicalUserAccount> service,
                string username, string password)
            {
                string email = $"{username}@example.com";
                var account = service.CreateAccount(username, password, email);

                var fromDb = service.GetByID(account.ID);

                fromDb.ShouldNotBe(null);
                fromDb.Username.ShouldBe(username);
                fromDb.Email.ShouldBe(email);
            }
        }

        public class TheAuthenticateMethod
        {
            public void CanAuthenticateAnAccount(UserAccountService<HierarchicalUserAccount> service,
                string username, string password)
            {
                string email = $"{username}@example.com";
                service.CreateAccount(username, password, email);

                service.Authenticate(username, password).ShouldBe(true);
            }
        }

        public class TheSetMobilePhone
        {
            public void Test(UserAccountService<HierarchicalUserAccount> service,
                string username, string password, string certThumb)
            {
                string email = $"{username}@example.com";
                var account = service.CreateAccount(username, password, email);

                service.AddCertificate(account.ID, certThumb, username);
            }
        }
    }
}