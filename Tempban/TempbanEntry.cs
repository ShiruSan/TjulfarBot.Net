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
            command.Prepare();
            command.Parameters.AddWithValue("@id", EntryID);
            command.Parameters.AddWithValue("@memberid", MemberID);
            command.Parameters.AddWithValue("@expires", Expires);
            command.Parameters.AddWithValue("@passed", false);
            command.ExecuteNonQuery();
        }

        public void Purge()
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "update TempBans set passed = @passed where id = @id";
            command.Prepare();
            command.Parameters["@passed"].Value = true;
            command.Parameters["@id"].Value = EntryID;
            command.ExecuteNonQuery();
        }
        
    }
}