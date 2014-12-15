using System;
using System.Linq;
using CentralServerShared;
using ServiceEntities;
using ServiceEntities.CentralService.v2;

namespace CentralService
{
    public class ServerAuthentication
    {
        private static Server CreateServer(ServerInfo info)
        {
            Server server = new Server()
            {
                Username = info.Username,
                Password = info.Password,
                ServerName = info.ServerName,
                DateCreated = DateTime.Now
            };

            return server;
        }

        public static int Authenticate(ServerInfo info)
        {
            Server server = new Server();
            int serverId = 0;
            try
            {
                DataModelContainer container = new DataModelContainer();

                var servers = from p in container.Servers
                              where p.Username == info.Username
                              select p;

                
                if (servers.Any())
                {
                    server = servers.First();
                    if (server.Password == info.Password)
                    {
                        if (server.ServerName != info.ServerName)
                        {
                            server.ServerName = info.ServerName;
                            container.SaveChanges();
                        }
                        return server.Id;
                    }
                }

                server = CreateServer(info);
                container.Servers.AddObject(server);
                container.SaveChanges();
                serverId = server.Id;
            }
            catch (Exception e)
            {
                Controller.WriteException("Authenticate", e.Message);
            }

            return serverId;
        }
    }
}