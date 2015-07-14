using System.Linq;
using BrockAllen.MembershipReboot;
using Shouldly;

namespace MembershipReboot.Postgres.Tests
{
    public class GroupServiceIntegrationTests
    {
        public class TheCreateMethod
        {
            public void CanCreateNewGroup(GroupService<HierarchicalGroup> service, string tenant, string name)
            {
                var group = service.Create(tenant, name);

                group.ShouldNotBe(null);
                group.Tenant.ShouldBe(tenant);
                group.Name.ShouldBe(name);
            }
        }

        public class TheDeleteMethod
        {
            public void CanDeleteGroup(GroupService<HierarchicalGroup> service, string tenant, string name)
            {
                var group = service.Create(tenant, name);

                service.Delete(group.ID);

                var fromDb = service.Get(group.ID);

                fromDb.ShouldBe(null);
            }

            public void CanDeleteGroupWithChildren(GroupService<HierarchicalGroup> service, string tenant, string name,
                string nparent1, string nparent2, string nchild)
            {
                var group = service.Create(tenant, name);
                var parent1 = service.Create(tenant, nparent1);
                var parent2 = service.Create(tenant, nparent2);
                var child = service.Create(tenant, nchild);

                service.AddChildGroup(parent1.ID, group.ID);
                service.AddChildGroup(parent2.ID, group.ID);
                service.AddChildGroup(parent2.ID, child.ID);

                service.Delete(group.ID);

                service.Get(group.ID).ShouldBe(null);
                service.Get(parent1.ID).Children.ShouldBeEmpty();
                parent2 = service.Get(parent2.ID);

                parent2.Children.ShouldNotBeEmpty();
                parent2.Children.Single().ChildGroupID.ShouldBe(child.ID);
            }
        }

        public class TheChangeNameMethod
        {
            public void CanRenameGroup(GroupService<HierarchicalGroup> service, string tenant, string name,
                string newName)
            {
                var group = service.Create(tenant, name);

                service.ChangeName(group.ID, newName);

                var fromDb = service.Get(group.ID);

                fromDb.ShouldNotBe(null);
                fromDb.Name.ShouldBe(newName);
            }
        }

        public class TheAddChildGroupMethod
        {
            public void CanAddChildToGroup(GroupService<HierarchicalGroup> service, string tenant, string name,
                string childName)
            {
                var group = service.Create(tenant, name);
                var child = service.Create(tenant, childName);

                service.AddChildGroup(group.ID, child.ID);

                var fromDb = service.Get(tenant, name);

                fromDb.ShouldNotBe(null);
                fromDb.Children.ShouldContain(x => x.ChildGroupID == child.ID);
            }
        }

        public class TheGetChildrenMethod
        {
            public void CanRetrieveChildren(GroupService<HierarchicalGroup> service, string tenant, string name,
                string childName1, string childName2)
            {
                var group = service.Create(tenant, name);
                var child1 = service.Create(tenant, childName1);
                var child2 = service.Create(tenant, childName2);
                service.AddChildGroup(group.ID, child1.ID);
                service.AddChildGroup(group.ID, child2.ID);
                group = service.Get(group.ID);

                var children = service.GetChildren(group);

                children.ShouldNotBeEmpty();
                children.Count().ShouldBe(2);
            }
        }
    }
}
