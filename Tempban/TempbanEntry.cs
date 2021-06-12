namespace TjulfarBot.Net.Tempban
{
    public class TempbanEntry
    {
        public int EntryID;
        public long MemberID;
        public long Expires;

        public TempbanEntry(int entryId, long memberId, long expires)
        {
            EntryID = entryId;
            MemberID = memberId;
            Expires = expires;
        }

        public void CreateDatabaseEntry()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "insert into TempBans values (@id, @memberid, @expires, @passed)";
            command.Parameters.AddWithValue("@id", EntryID);
            command.Parameters.AddWithValue("@memberid", MemberID);
            command.Parameters.AddWithValue("@expires", Expires);
            command.Parameters.AddWithValue("@passed", false);
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public void Purge()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "update TempBans set passed = @passed where id = @id";
            command.Parameters.AddWithValue("@passed", true);
            command.Parameters.AddWithValue("@id", EntryID);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
    }
}