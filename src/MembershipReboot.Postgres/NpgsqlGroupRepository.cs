using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using BrockAllen.MembershipReboot;
using Npgsql;
using NpgsqlTypes;

namespace MembershipReboot.Postgres
{
    public class NpgsqlGroupRepository : IGroupRepository<HierarchicalGroup>
    {
        private readonly NpgsqlConnection _conn;

        public NpgsqlGroupRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public HierarchicalGroup Create()
        {
            return new HierarchicalGroup();
        }

        public void Add(HierarchicalGroup item)
        {
            const string query = "INSERT INTO " +
                                 "groups (id, tenant, name, created, last_updated, children) " +
                                 "VALUES " +
                                 "(@id, @tenant, @name, @created, @lastUpdated, @children)";

            _conn.ExecuteCommand(query, cmd =>
            {
                var children = item.Children.Select(x => x.ChildGroupID).ToArray();
                var childrenDbType = (NpgsqlDbType.Uuid | NpgsqlDbType.Array);

                cmd.Parameters.AddWithValue("id", item.ID);
                cmd.Parameters.AddWithValue("tenant", item.Tenant);
                cmd.Parameters.AddWithValue("name", item.Name);
                cmd.Parameters.AddWithValue("created", item.Created);
                cmd.Parameters.AddWithValue("lastUpdated", item.LastUpdated);
                cmd.Parameters.AddWithValue("children", childrenDbType, children);

                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected;
            });
        }

        public void Remove(HierarchicalGroup item)
        {
            const string query = "DELETE FROM groups " +
                                 "WHERE id = @id";

            _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("id", item.ID);

                return cmd.ExecuteNonQuery();
            });
        }

        public void Update(HierarchicalGroup item)
        {
            const string query = "UPDATE groups " +
                           "SET " +
                           " tenant = @tenant, " +
                           " name = @name, " +
                           " last_updated = @lastUpdated," +
                           " children = @children " +
                           "WHERE id = @id";

            _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("id", item.ID);
                cmd.Parameters.AddWithValue("tenant", item.Tenant);
                cmd.Parameters.AddWithValue("name", item.Name);
                cmd.Parameters.AddWithValue("lastUpdated", item.LastUpdated);
                cmd.Parameters.AddWithValue("children", item.Children.Select(x => x.ChildGroupID).ToArray());

                return cmd.ExecuteNonQuery();
            });
        }

        public HierarchicalGroup GetByID(Guid id)
        {
            const string query = "SELECT * " +
                           "FROM groups " +
                           "WHERE id = @id";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Read(reader);
                    }

                    return null;
                }
            });
        }

        public HierarchicalGroup GetByName(string tenant, string name)
        {
            const string query = "SELECT * " +
                           "FROM groups " +
                           "WHERE tenant = @tenant " +
                           " AND name = @name";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("tenant", tenant);
                cmd.Parameters.AddWithValue("name", name);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Read(reader);
                    }

                    return null;
                }
            });
        }

        public IEnumerable<HierarchicalGroup> GetByIDs(Guid[] ids)
        {
            const string query = "SELECT * " +
                           "FROM groups " +
                           "WHERE id = ANY(@ids)";

            return _conn.ExecuteCommand(query, cmd =>
            {
                cmd.Parameters.AddWithValue("ids", ids);

                using (var reader = cmd.ExecuteReader())
                {
                    var resultList = new List<HierarchicalGroup>();

                    while (reader.Read())
                    {
                        resultList.Add(Read(reader));
                    }

                    return resultList;
                }
            });
        }

        public IEnumerable<HierarchicalGroup> GetByChildID(Guid childGroupID)
        {
            const string query = "SELECT * " +
                            "FROM groups " +
                            "WHERE children && @children";

            return _conn.ExecuteCommand(query, cmd =>
            {
                var children = new[] { childGroupID };

                cmd.Parameters.AddWithValue("children", children);

                using (var reader = cmd.ExecuteReader())
                {
                    var resultList = new List<HierarchicalGroup>();

                    while (reader.Read())
                    {
                        resultList.Add(Read(reader));
                    }

                    return resultList;
                }
            });
        }

        private HierarchicalGroup Read(IDataReader reader)
        {
            var group = Create();
            int idOrdinal = reader.GetOrdinal("id");
            int tenantOrdinal = reader.GetOrdinal("tenant");
            int nameOrdinal = reader.GetOrdinal("name");
            int createdOrdinal = reader.GetOrdinal("created");
            int lastUpdatedOrdinal = reader.GetOrdinal("last_updated");
            int childrenOrdinal = reader.GetOrdinal("children");

            Set(x => x.ID, group, reader.GetGuid(idOrdinal));
            Set(x => x.Tenant, group, reader.GetString(tenantOrdinal));
            Set(x => x.Name, group, reader.GetString(nameOrdinal));
            Set(x => x.Created, group, reader.GetDateTime(createdOrdinal));
            Set(x => x.LastUpdated, group, reader.GetDateTime(lastUpdatedOrdinal));
            var children = reader.GetValue(childrenOrdinal) as Guid[];
            children.Select(x =>
            {
                var child = new GroupChild();

                Set(g => g.ChildGroupID, child, x);

                return child;
            }).ToList().ForEach(x => group.GroupCollection.Add(x));

            return group;
        }

        private void Set<TTarget, T>(Expression<Func<TTarget, T>> prop, TTarget target, T value)
        {
            var property = ((MemberExpression)prop.Body).Member;
            var type = typeof(TTarget);

            type.GetProperty(property.Name).SetValue(target, value);
        }
    }
}
