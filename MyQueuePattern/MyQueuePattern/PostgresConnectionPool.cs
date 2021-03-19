using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyQueuePattern
{
    public class PostgresConnectionPool
    {
        public Queue<NpgsqlConnection> m_connections;
        private static PostgresConnectionPool INSTANCE;
        private static object key = new object();
        private object conn_key = new object();
        public const int MAX_CONN = 40;
        // should be in config file !!
        private const string conn_string = "Host=localhost;Username=postgres;Password=admin;Database=postgres";

        public static PostgresConnectionPool Instance
        {
            get
            {
                if (INSTANCE == null)
                {
                    lock (key)
                    {
                        if (INSTANCE == null)
                        {
                            INSTANCE = new PostgresConnectionPool();
                            INSTANCE.Init(MAX_CONN);
                            return INSTANCE;
                        }
                    }
                }
                return INSTANCE;
            }
        }

        private PostgresConnectionPool()
        {
            m_connections = new Queue<NpgsqlConnection>(MAX_CONN);
        }

        private void Init(int max_connections)
        {
            for (int i = 0; i < max_connections; i++)
            {
                m_connections.Enqueue(new 
                    NpgsqlConnection(conn_string));
            }
        }

        public NpgsqlConnection GetConnection()
        {
            lock (conn_key)
            {
                while (m_connections.Count == 0)
                {
                    Monitor.Wait(conn_key);
                }
                var conn = m_connections.Dequeue();
                conn.Open();
                return conn;
            }
        }

        public void ReturnConnection(NpgsqlConnection conn)
        {
            lock (conn_key)
            {
                if (m_connections.Count < MAX_CONN)
                {
                    try
                    {
                        if (conn != null)
                        {
                            conn.Dispose();
                        }
                    }
                    catch { }
                    conn = new NpgsqlConnection(conn_string);
                    m_connections.Enqueue(conn);
                }
                Monitor.Pulse(conn_key);
            }
        }

        public void RestartPool()
        {
            lock (conn_key)
            {
                m_connections.Clear();

                for (int i = 0; i < MAX_CONN; i++)
                {
                    m_connections.Enqueue(new NpgsqlConnection(conn_string));
                }
                Monitor.PulseAll(conn_key);
            }
        }

        public bool TestDbConnection()
        {
            //my_logger.Debug("Testing db access");
            try
            {
                using (var my_conn = new NpgsqlConnection(conn_string))
                {
                    my_conn.Open();
                    //my_logger.Debug("Testing db access. succeed!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                //my_logger.Fatal($"Testing db access. Failed!. Error: {ex}");
                return false;
            }
        }
    }
}
