using System;

namespace TjulfarBot.Net.Economy
{
    public class EcoManager
    {
        private static EcoManager _instance;

        public static EcoManager Get()
        {
            if (_instance == null) _instance = new EcoManager();
            return _instance;
        }

        private int GetNewestID()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Eco`";
            using var reader = command.ExecuteReader();
            int i = 0;
            while (reader.Read()) i = reader.GetInt32(0);
            return i + 1;
        }

        public bool HasPocketMoney(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Eco` where `userid` = @userid && `type` = @type";
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@type", "pocket");
            command.Prepare();
            using var reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public bool HasPocketMoney(ulong userid, int money)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Eco` where `userid` = @userid && `type` = @type";
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@type", "pocket");
            command.Prepare();
            using var reader = command.ExecuteReader();
            return reader.GetInt32(2) <= money;
        }

        public bool HasBankAccount(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Eco` where `userid` = @userid && `type` = @type";
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@type", "bank");
            command.Prepare();
            using var reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public bool HasBankMoney(ulong userid, int money)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Eco` where `userid` = @userid && `type` = @type";
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@type", "bank");
            command.Prepare();
            using var reader = command.ExecuteReader();
            return reader.GetInt32(2) <= money;
        }

        public void CreatePocketAccount(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "insert into `Eco` values(@id, @userid, @money, @type, @daily)";
            command.Parameters.AddWithValue("@id", GetNewestID());
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@money", 100);
            command.Parameters.AddWithValue("@type", "pocket");
            command.Parameters.AddWithValue("@daily", DateTime.Now);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        public void CreateBankAccount(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "insert into `Eco` values(@id, @userid, @money, @type, @daily)";
            command.Parameters.AddWithValue("@id", GetNewestID());
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@money", 0);
            command.Parameters.AddWithValue("@type", "pocket");
            command.Parameters.AddWithValue("@daily", null);
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public void AddPocketMoney(ulong userid, int count)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "update `Eco` set `money` = `money` + @count where `userid` = @userid && `type` = @type";
            command.Parameters.AddWithValue("@count", count);
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@type", "pocket");
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public void RemovePocketMoney(ulong userid, int count)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "update `Eco` set `money` = `money` - @count where `userid` = @userid && `type` = @type";
            command.Parameters.AddWithValue("@count", count);
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@type", "pocket");
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        public void AddBankMoney(ulong userid, int count)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "update `Eco` set `money` = `money` + @count where `userid` = @userid && `type` = @type";
            command.Parameters.AddWithValue("@count", count);
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@type", "bank");
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public void RemoveBankMoney(ulong userid, int count)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "update `Eco` set `money` = `money` - @count where `userid` = @userid && `type` = @type";
            command.Parameters.AddWithValue("@count", count);
            command.Parameters.AddWithValue("@userid", userid);
            command.Parameters.AddWithValue("@type", "bank");
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public void RemoveAccounts(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "delete from `Eco` where `userid` = @userid";
            command.Parameters.AddWithValue("@userid", userid);
            command.Prepare();
            command.ExecuteNonQuery();
        }

    }
}