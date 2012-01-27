using System.ComponentModel.Composition.Hosting;
using Raven.Client.Embedded;
using Xunit;

namespace RavenCrypt.Tests
{
    public class CryptTest
    {
        private readonly EmbeddableDocumentStore documentStore;

        public CryptTest()
        {
            documentStore = new EmbeddableDocumentStore { RunInMemory = true };
            documentStore.Configuration.Catalog = new AggregateCatalog
            {
                Catalogs =
                    {
                        new AssemblyCatalog(typeof (DocumentCodec).Assembly)
                    }
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