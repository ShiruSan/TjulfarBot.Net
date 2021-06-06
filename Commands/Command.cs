using Discord;
using TjulfarBot.Net.Managers;

namespace TjulfarBot.Net.Commands
{
    public abstract class Command
    {
        public string name;
        public object Permission;

        protected Command(string name, object permission = null)
        {
            this.name = name;
            this.Permission = permission;
        }

        public abstract void OnCommand(CommandContext ctx);

    }
}