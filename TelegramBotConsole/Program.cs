using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotCore.Manager;

namespace TelegramBotCore
{
    class Program
    {
        private static IConfiguration Configuration;
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            Configuration = builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Appsettings.json")
            .Build();

            var serviceProvider = new ServiceCollection()
            .AddDbContext<BotDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            })
            .AddTransient<IRepository, Repository>()
            .BuildServiceProvider();
            
            // var dbContext = serviceProvider.GetRequiredService<BotDbContext>();
            // dbContext.Database.Migrate();
            
            BotManager botManager = new BotManager( serviceProvider.GetService<IRepository>(), 
                                                    Configuration.GetSection("BotToken").Value, 
                                                    Directory.GetCurrentDirectory() + "/MessageText.json",
                                                    Convert.ToInt32( Configuration.GetSection("DailyQuestionLimit").Value));

            var me = botManager.Bot.GetMeAsync().Result;
            Console.Title = me.Username;

            botManager.Bot.OnMessage += botManager.BotOnMessageReceived;
            botManager.Bot.OnMessageEdited += botManager.BotOnMessageReceived;
            botManager.Bot.OnReceiveError += botManager.BotOnReceiveError;

            botManager.Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            botManager.Bot.StopReceiving();
        }

    }
}

