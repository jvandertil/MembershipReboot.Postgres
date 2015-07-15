using System;
using System.Data;
using System.IO;
using System.Reflection;
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
            JsonProperty prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }

                prop.ShouldSerialize = instance => prop.Writable;
            }

            return prop;
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
                Formatting = Formatting.Indented,
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

            _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("id", item.ID);

                return cmd.ExecuteNonQuery();
            });
        }

        public void Update(HierarchicalUserAccount item)
        {
            const string query = "UPDATE useraccounts " +
                                 "SET tenant = @tenant, " +
                                 " username = @username, " +
                                 " email = @email, " +
                                 " hashed_password = @password, " +
                                 " account = @account " +
                                 "WHERE id = @id";

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

        public HierarchicalUserAccount GetByID(Guid id)
        {
            const string query = "SELECT account " +
                                 "FROM useraccounts " +
                                 "WHERE id = @id";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Deserialize(reader);
                    }

                    return null;
                }
            });
        }

        public HierarchicalUserAccount GetByUsername(string username)
        {
            const string query = "SELECT account " +
                                 "FROM useraccounts " +
                                 "WHERE username = @username";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Deserialize(reader);
                    }

                    return null;
                }
            });
        }

        public HierarchicalUserAccount GetByUsername(string tenant, string username)
        {
            const string query = "SELECT account " +
                                 "FROM useraccounts " +
                                 "WHERE tenant = @tenant " +
                                 " AND username = @username";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("tenant", tenant);
                cmd.Parameters.AddWithValue("username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Deserialize(reader);
                    }

                    return null;
                }
            });
        }

        public HierarchicalUserAccount GetByEmail(string tenant, string email)
        {
            const string query = "SELECT account " +
                                 "FROM useraccounts " +
                                 "WHERE tenant = @tenant " +
                                 " AND email = @email";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("tenant", tenant);
                cmd.Parameters.AddWithValue("email", email);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Deserialize(reader);
                    }

                    return null;
                }
            });
        }

        public HierarchicalUserAccount GetByMobilePhone(string tenant, string phone)
        {
            const string query = "SELECT account " +
                     "FROM useraccounts " +
                     "WHERE tenant = @tenant" +
                     " AND account->>'MobilePhoneNumber'= @phone";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("tenant", tenant);
                cmd.Parameters.AddWithValue("phone", phone);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Deserialize(reader);
                    }

                    return null;
                }
            });
        }

        public HierarchicalUserAccount GetByVerificationKey(string key)
        {
            const string query = "SELECT account " +
                                 "FROM useraccounts " +
                                 "WHERE account->>'VerificationKey'= @key";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("key", key);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Deserialize(reader);
                    }

                    return null;
                }
            });
        }

        public HierarchicalUserAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            const string query = "SELECT account " +
                                 "FROM useraccounts u " +
                                 "INNER JOIN " +
                                 " (SELECT id," +
                                 " (jsonb_array_elements(account->'LinkedAccountCollection')->>'ProviderName' = @provider) as hasProvider," +
                                 " (jsonb_array_elements(account->'LinkedAccountCollection')->>'ProviderAccountID' = @id) as accountExists" +
                                 " FROM useraccounts) t " +
                                 "ON u.id = t.id " +
                                 "WHERE t.hasProvider = true " +
                                 "  AND t.accountExists = true";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("provider", provider);
                cmd.Parameters.AddWithValue("id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Deserialize(reader);
                    }

                    return null;
                }
            });
        }

        public HierarchicalUserAccount GetByCertificate(string tenant, string thumbprint)
        {
            const string query = "SELECT account " +
                                 "FROM useraccounts u " +
                                 "INNER JOIN " +
                                 " (SELECT id, " +
                                 " (jsonb_array_elements(account->'UserCertificateCollection')->>'Thumbprint' = @thumbprint) as hasThumbprint " +
                                 " FROM useraccounts) t " +
                                 "ON u.id = t.id " +
                                 "WHERE t.hasThumbprint = true";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("thumbprint", thumbprint);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Deserialize(reader);
                    }

                    return null;
                }
            });
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

        private HierarchicalUserAccount Deserialize(IDataReader reader)
        {
            int accountOrdinal = reader.GetOrdinal("account");
            var serialized = reader.GetString(accountOrdinal);
            return Deserialize(serialized);
        }
    }
}