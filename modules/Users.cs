using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace MothBot.modules
{
    internal static class Users
    {
        public const string PATH_USERS = "../data/users.json";
        private static readonly List<User> users = new List<User>();

        static Users()
        {
            string fileData = Data.Files_Read(PATH_USERS);
            if (fileData == null || fileData.Length == 0)
            {
                Logging.LogtoConsoleandFile($"No user data found at {PATH_USERS}, running with empty list of users.");
                users.Clear();
            }
            else
            {
                List<User> fileUsers = JsonConvert.DeserializeObject<List<User>>(fileData);
                foreach (User user in fileUsers)
                    users.Add(user);
            }
        }

        public static Task Client_Ready()
        {
            Master.client.UserJoined += Client_UserJoined;
            Master.client.JoinedGuild += Client_JoinedGuild;
            Master.client.MessageReceived += Client_MessageReceived;
            return Task.CompletedTask;
        }

        public static User GetUser(string Username)
        {
            foreach (User user in users)
                if (user.Username.ToUpperInvariant() == Username.ToUpperInvariant())
                    return user;
            return null;
        }

        public static bool GetUser(string Username, out User userout)
        {
            foreach (User user in users)
                if (user.Username.ToUpperInvariant() == Username.ToUpperInvariant())
                {
                    userout = user;
                    return true;
                }
            userout = null;
            return false;
        }

        public static User GetUser(ulong ID)
        {
            foreach (User user in users)
                if (user.Id == ID)
                    return user;
            return null;
        }

        public static bool GetUser(ulong ID, out User userout)
        {
            foreach (User user in users)
                if (user.Id == ID)
                {
                    userout = user;
                    return true;
                }
            userout = null;
            return false;
        }

        public static bool IsBanned(ulong id)
        {
            if (GetUser(id, out User user))
                return user.isBanned;
            return false;
        }

        public static bool IsBanned(IUser usr)
        {
            return IsBanned(usr.Id);
        }

        public static void SaveUsers()
        {
            List<User> newUsers = new List<User>();
            foreach (User user in users)
            {
                if (!newUsers.Contains(user))
                    newUsers.Add(user);
            }
            users.Clear();
            users.AddRange(newUsers);
            Data.Files_Write(PATH_USERS, JsonConvert.SerializeObject(users, Formatting.Indented));
        }

        private static bool AddUser(IUser user)
        {
            if (GetUser(user.Id) == null)
            {
                users.Add(new User(user));
                SaveUsers();
                return true;
            }
            return false;
        }

        private static async Task Client_JoinedGuild(SocketGuild guild)
        {
            guild.DownloadUsersAsync().Wait();
            foreach (IGuildUser user in guild.Users)
                AddUser(user);
            if (IsBanned(guild.Owner))
            {
                await guild.LeaveAsync();
                await Logging.LogtoConsoleandFileAsync($"Attempted to join guild owned by banned user {guild.OwnerId}, left guild.");
            }
        }

        private static Task Client_MessageReceived(SocketMessage msg)
        {
            AddUser(msg.Author);
            return Task.CompletedTask;
        }

        private static Task Client_UserJoined(SocketGuildUser usr)
        {
            AddUser(usr);
            return Task.CompletedTask;
        }

        public class User
        {
            public readonly ulong Id;
            public readonly string Username;
            public bool isBanned;

            public User(IUser user)
            {
                Id = user.Id;
                Username = user.Username;
                isBanned = false;
            }

            [JsonConstructor]
            public User(ulong id, string username, bool isbanned)
            {
                Id = id;
                isBanned = isbanned;
                Username = username;
            }
        }
    }
}