﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie;
using Ploeh.AutoFixture.Kernel;
using Fixture = Ploeh.AutoFixture.Fixture;

namespace MembershipReboot.Postgres.Tests.Fixie
{
    public class NestedClassPerMethodConvention : Convention
    {
        public NestedClassPerMethodConvention()
        {
            Classes
                .Where(x => x.IsNested && x.DeclaringType.Name.EndsWith("Tests"))
                .Where(t => t.GetConstructors().All(ci => ci.GetParameters().Length == 0));

            Methods.Where(mi => mi.IsPublic && (mi.IsVoid() || mi.IsAsync()));

            Parameters.Add(FillFromFixture);
        }

        private IEnumerable<object[]> FillFromFixture(MethodInfo method)
        {
            var fixture = new Fixture();

            fixture.Customize(new StructureMapCustomization());

            yield return GetParameterData(method.GetParameters(), fixture);
        }

        private object[] GetParameterData(ParameterInfo[] parameters, Fixture fixture)
        {
            return parameters
                .Select(p => new SpecimenContext(fixture).Resolve(p))
                .ToArray();
        }
    }
}