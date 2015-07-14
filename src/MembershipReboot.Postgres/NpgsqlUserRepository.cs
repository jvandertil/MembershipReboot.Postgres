using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Hierarchical;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Npgsql;

namespace MembershipReboot.Postgres
{
    class WritablePropertiesOnlyResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(HierarchicalUserAccount))
            {
                var info = member as PropertyInfo;
                if (info != null)
                {
                    property.ShouldSerialize = instance => info.CanWrite;
                }

                //member.
            }

            return property;
        }
    }

    public class NpgsqlUserAccountRepository : IUserAccountRepository<HierarchicalUserAccount>
    {
        private readonly NpgsqlConnection _conn;
        private readonly JsonSerializer _serializer;

        public NpgsqlUserAccountRepository(NpgsqlConnection conn)
        {
            _conn = conn;

            _serializer = new JsonSerializer()
            {
                ContractResolver = new WritablePropertiesOnlyResolver(),
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented
            };
        }

        public HierarchicalUserAccount Create()
        {
            return new HierarchicalUserAccount();
        }

        public void Add(HierarchicalUserAccount item)
        {
            const string query = "INSERT INTO " +
                                 "useraccounts(id, tenant, username, email, hashed_password, account) " +
                                 "VALUES (@id, @tenant, @username, @email, @password, @account);";

            _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("id", item.ID);
                cmd.Parameters.AddWithValue("tenant", item.Tenant);
                cmd.Parameters.AddWithValue("username", item.Username);
                cmd.Parameters.AddWithValue("email", item.Email);
                cmd.Parameters.AddWithValue("password", item.HashedPassword);
                cmd.Parameters.AddWithValue("account", Serialize(item));

                return cmd.ExecuteNonQuery();
            });
        }

        public void Remove(HierarchicalUserAccount item)
        {
            const string query = "DELETE FROM useraccounts " +
                                 "WHERE id = @id";

            throw new NotImplementedException();
        }

        public void Update(HierarchicalUserAccount item)
        {
            const string query = "UPDATE useraccounts " +
                                 "SET tenant = @tenant, " +
                                 "SET username = @username, " +
                                 "SET email = @email, " +
                                 "SET hashed_password = @password, " +
                                 "SET account = @account " +
                                 "WHERE id = @id";

            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByID(Guid id)
        {
            const string query = "SELECT * FROM useraccounts WHERE id = @id";
            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByUsername(string username)
        {
            const string query = "SELECT * FROM useraccounts WHERE username = @username";
            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByUsername(string tenant, string username)
        {
            const string query = "SELECT * FROM useraccounts WHERE tenant = @tenant AND username = @username";
            return null;
            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByEmail(string tenant, string email)
        {
            const string query = "SELECT * FROM useraccounts WHERE tenant = @tenant AND email = @email";
            return null;
            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByMobilePhone(string tenant, string phone)
        {
            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByVerificationKey(string key)
        {
            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByCertificate(string tenant, string thumbprint)
        {
            throw new NotImplementedException();
        }

        private string Serialize(HierarchicalUserAccount item)
        {
            using (var writer = new StringWriter())
            {
                _serializer.Serialize(writer, item);

                writer.Flush();

                return writer.ToString();
            }
        }

        private HierarchicalUserAccount Deserialize(string item)
        {
            using (var reader = new JsonTextReader(new StringReader(item)))
            {
                return _serializer.Deserialize<HierarchicalUserAccount>(reader);
            }
        }
    }
}