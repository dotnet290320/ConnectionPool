using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyQueuePattern
{

    // THIS ID DEMO CODE

    // PLEASE USE PostgresConnectionPool class !!


    public class MyConnecion { }
    public class MyConnPool
    {
        public Queue<MyConnecion> m_connections;
        private static MyConnPool INSTANCE;
        private static object key = new object();
        private object conn_key = new object();
        public const int MAX_CONN = 40;

        public static MyConnPool Instance
        {
            get
            {
                if (INSTANCE == null)
                {
                    lock(key)
                    {
                        if (INSTANCE == null)
                        {
                            INSTANCE = new MyConnPool();
                            INSTANCE.Init(MAX_CONN);
                            return INSTANCE;
                        }
                    }
                }
                return INSTANCE;
            }
        }

        private void Init(int max_connections)
        {
            for (int i = 0; i < max_connections; i++)
            {
                m_connections.Enqueue(new MyConnecion());
            }
        }

        public MyConnecion GetConnection()
        {
            lock (conn_key)
            {
                while (m_connections.Count == 0)
                {
                    //Thread.Sleep(100);
                    Monitor.Wait(conn_key);
                    // after waiting room
                    // wait for connection
                    // sleep until connection arrives
                }
                var conn = m_connections.Dequeue();
                return conn;
            }
        }

        public void ReturnConnection(MyConnecion conn)
        {
            lock (conn_key)
            {
                if (m_connections.Count < MAX_CONN)
                {
                    m_connections.Enqueue(conn);
                }
                Monitor.Pulse(conn_key);
            }
        }

        public void FOO()
        {
            lock (conn_key)
            {
                m_connections.Clear();

                for (int i = 0; i < MAX_CONN; i++)
                {
                    m_connections.Enqueue(new MyConnecion());
                }
                Monitor.PulseAll(conn_key); 
            }
        }

}
}
