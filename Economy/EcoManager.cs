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

        public bool HasMoney(ulong userid)
        {
            using var connection = Program.instance.DatabaseManager.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "select * from `Eco` where ``";
            return false;
        }
        
    }
}