using UCS.Helpers;

namespace UCS.Database
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public class Mysql : DbContext
    {
        private static string ConnectionString = $@"
            metadata=res://*/Database.ucsdb.csdl
            |res://*/Database.ucsdb.ssdl
            |res://*/Database.ucsdb.msl;
            provider=MySql.Data.MySqlClient;
            provider connection string='
                server={Utils.ParseConfigString("MysqlIPAddress")};
                port={Utils.ParseConfigInt("MysqlPort")};
                user id={Utils.ParseConfigString("MysqlUsername")};
                password={Utils.ParseConfigString("MysqlPassword")};        
                CharSet=utf8mb4;
                persistsecurityinfo=True;
                database={Utils.ParseConfigString("MysqlDatabase")};
                Convert Zero Datetime=True;
                Allow Zero Datetime=True;
        '";
        public Mysql() : base(ConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }

        public virtual DbSet<Clan> Clan { get; set; }
        public virtual DbSet<Player> Player { get; set; }
    }
}
