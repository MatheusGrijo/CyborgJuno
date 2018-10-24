using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace CyborgJuno
{
    class Program
    {
        static TelegramBotClient Bot = null;
        static void Main(string[] args)
        {

            Bot = new TelegramBotClient("");
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");


            while(true)
            {
                //



                if (DateTime.Now.ToString("HH") == "08" || DateTime.Now.ToString("HH") == "12" || DateTime.Now.ToString("HH") == "16" || DateTime.Now.ToString("HH") == "20")
                {
                    Bot.SendTextMessageAsync("@ejuno", getCotacao());
                    Console.WriteLine("sleep 2h");
                    System.Threading.Thread.Sleep(60000 * 60);
                    System.Threading.Thread.Sleep(60000 * 60);
                }


                System.Threading.Thread.Sleep(60000);

            }


            Console.ReadLine();
            Bot.StopReceiving();


        }


        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) return;

            switch (message.Text.Split(' ').First())
            {
             
                case "/cotacao":


                    await Bot.SendTextMessageAsync(
                        message.Chat.Id,
                        getCotacao(),
                        replyMarkup: new ReplyKeyboardRemove());
                    break;

                default:
                    const string usage = @"
Usage:
/cotacao   - receba a cotacao


";

                    await Bot.SendTextMessageAsync(
                        message.Chat.Id,
                        usage,
                        replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }


        static string getCotacao()
        {
            string ret = get("https://api.e-juno.com.br/api/market?instrument=BTC/BRL");
            ret = System.Text.RegularExpressions.Regex.Split(ret, "\"last\"")[1].Split(',')[0].ToString().Replace("\"","").Replace(":","").Split('.')[0];
            return "A cotação atual do BITCOIN na exchange e-Juno é de R$ "+ret+" acesse o site www.e-juno.com.br";
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await Bot.AnswerCallbackQueryAsync(
                callbackQuery.Id,
                $"Received {callbackQuery.Data}");

            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"Received {callbackQuery.Data}");
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
           
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }

        public static string get(String url)
        {
            try
            {

                String r = "";
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
                httpWebRequest.Method = "GET";
                var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var responseStream = httpWebResponse.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream);
                    r = streamReader.ReadToEnd();
                }
                if (responseStream != null) responseStream.Close();
                //Console.WriteLine(r);
                return r;
            }
            catch (WebException ex)
            {
                return null;
            }
        }


    }
}
