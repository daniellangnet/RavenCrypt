using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Database.Config;
using Raven.Database.Extensions;
using Raven.Database.Server;
using Raven.Json.Linq;
using Raven.Server;
using Xunit;

namespace RavenCrypt.Tests
{
    public class CryptTest
    {
        private readonly DocumentStore documentStore;
        private readonly RavenDbServer ravenDbServer;

        public CryptTest()
        {
            string path = Path.GetDirectoryName(Assembly.GetAssembly(typeof (CryptTest)).CodeBase);
            path = Path.Combine(path, "TestDb").Substring(6);
            
            IOExtensions.DeleteDirectory(path);

            ravenDbServer = new RavenDbServer(
                new RavenConfiguration
                {
                    Port = 8079,
                    DataDirectory = path,
                    Catalog =
                        {
                            Catalogs =
                                {
                                    new AssemblyCatalog(typeof (DocumentCodec).Assembly)
                                }
                        },
                    AnonymousUserAccessMode = AnonymousUserAccessMode.All
                });

            documentStore = new DocumentStore
            {
                Url = "http://localhost:8079"
            };
            documentStore.Initialize();
        }

        [Fact]
        public void Encryption_and_decryption_works()
        {
            using (var session = documentStore.OpenSession())
            {
                session.Store(new User
                {
                    Name = "Daniel Lang"
                });

                session.SaveChanges();
            }

            using (var session = documentStore.OpenSession())
            {
                var user = session.Load<User>("users/1");
                Assert.Equal("Daniel Lang", user.Name);
            }
        }

        public class User
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}