namespace MembershipReboot.Postgres
{
    using System;
    using System.Data;
    using System.Threading.Tasks;

    using Npgsql;

    internal static class TransactionHelper
    {
        public static T ExecuteCommand<T>(this NpgsqlConnection conn,
            string commandText, Func<NpgsqlCommand, T> func)
        {
            T result;

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using (var tx = conn.BeginTransaction())
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = commandText;

                    result = func(cmd);
                }

                tx.Commit();
            }

            return result;
        }
    }
}