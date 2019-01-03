using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console_Telegram_Bot;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace newYearQuestTelegramBotConsoleApp
{
    public static class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("727991360:AAE0SzvC22CqsPabpBp7lBoVxBpuEdn1tDU");
        // Dictionary of saved steps per chat Id
        private static Dictionary<long, QuestSteps> currentStepPerUserDictionary;
        // Dictionary with answers per step
        private static readonly Dictionary<QuestSteps, string> answersPerStepDictionary = new Dictionary<QuestSteps, string>()
        {
            {QuestSteps.ConnectDots, "телефон"},
            {QuestSteps.Rebus, "santa"},
            {QuestSteps.AudioNumber, "124233"}
        };
        // Dictionary with file pathes per step
        private static readonly Dictionary<QuestSteps, string> filePathPerStepDictionary = new Dictionary<QuestSteps, string>()
        {
            {QuestSteps.Greeting, @"..\..\elf_greeting.jpg"},
            {QuestSteps.ConnectDots, @"..\..\rebus.jpg"},
            {QuestSteps.Rebus, @"..\..\audio.mp3"},
            {QuestSteps.AudioNumber, @"..\..\crossword.jpg"},
            {QuestSteps.Selfie, @"..\..\present.jpg"}
        };

        private static string elfOfTheYearPhoto = @"..\..\elf_of_the_year.jpg";
        private static string actorPhoto = @"..\..\actor.jpg";
        
        public static void Main(string[] args)
        {
            // Initialize logger
            Logger.InitLogger();
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
            Logger.Log.Info(String.Format("Started bot: {0}", me.Username));

            currentStepPerUserDictionary = new Dictionary<long, QuestSteps>();

            // Events subscription
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(new List<UpdateType>().ToArray());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            Logger.Log.Info(String.Format("Stopped bot: {0}", me.Username));
            Bot.StopReceiving();
        }


        private static async void SendMessageDependingOnCurrentStep(QuestSteps currentStep, long chatId, Message message)
        {
            switch (currentStep)
            {
                case QuestSteps.Greeting:
                    // Send message with explanation
                    await Bot.SendChatActionAsync(chatId, ChatAction.Typing);

                    await Bot.SendTextMessageAsync(chatId,
                        "Привет, я эльф Йорик, я помогаю Санте доставлять подарки.");

                    // Send photo
                    using (
                        var fileStream = new FileStream(filePathPerStepDictionary[currentStep], FileMode.Open,
                            FileAccess.Read, FileShare.Read))
                    {
                        await Bot.SendPhotoAsync(chatId, fileStream, "");
                    }

                    await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
                    await Task.Delay(10000);
                    await Bot.SendTextMessageAsync(chatId,
                        "В этом году я могу стать работником года, но для этого мне надо доставить последний подарок.\n" +
                        "Вот только я не могу вспомнить где он...");

                    await Bot.SendTextMessageAsync(chatId,
                      "Под Новый Год от волнения я становлюсь немного рассеянным и забывчивым.");

                    await Bot.SendTextMessageAsync(chatId,
                        "Я помню, что утром я наряжал свою маленькую ёлку.\n" +
                        "И одной из игрушек была аппетитная конфета.\n" +
                        "Не тормозите, подкрепитесь!");

                    await Bot.SendTextMessageAsync(chatId,
                     "Потом я подошел к какому-то предмету. Помогите вспомнить к какому.");

                    // Change step 
                    currentStepPerUserDictionary[chatId] = QuestSteps.ConnectDots;
                    break;

                case QuestSteps.ConnectDots:
                    if (message.Text!=null && message.Text.Split(' ').First().ToLower() == answersPerStepDictionary[currentStep])
                    {
                        await Bot.SendTextMessageAsync(chatId,
                            "Точно, раздался телефонный звонок.");

                        await Bot.SendTextMessageAsync(chatId,
                            "Кто звонил? Не помню...");

                        // Send photo
                        using (
                            var fileStream = new FileStream(filePathPerStepDictionary[currentStep], FileMode.Open,
                                FileAccess.Read, FileShare.Read))
                        {
                            await Bot.SendPhotoAsync(chatId, fileStream, "");
                        }

                        // Change step 
                        currentStepPerUserDictionary[chatId] = QuestSteps.Rebus;
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(chatId,
                            "Хмм, что-то мне это ни о чем не говорит...");
                    }
                    break;

                case QuestSteps.Rebus:
                    if (message.Text != null 
                        && (message.Text.Split(' ').First().ToLower() == answersPerStepDictionary[currentStep] || message.Text.Split(' ').First().ToLower() == "санта"))
                    {
                        await Bot.SendTextMessageAsync(chatId,
                            "Это был Санта(точнее, его робот-помощник).Он напомнил мне порядковый номер подарка, который я должен доставить.");

                        await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
                        await Task.Delay(2000);
                        await Bot.SendTextMessageAsync(chatId,
                            "Никак не могу вспомнить что за номер.");

                        // Send audio
                        using (
                            var fileStream = new FileStream(filePathPerStepDictionary[currentStep], FileMode.Open,
                                FileAccess.Read, FileShare.Read))
                        {
                            await Bot.SendAudioAsync(chatId, fileStream, "");
                        }

                        // Change step 
                        currentStepPerUserDictionary[chatId] = QuestSteps.AudioNumber;
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(chatId,
                            "Нет, я не знаю кто это");
                    }
                    break;

                case QuestSteps.AudioNumber:
                    if (message.Text != null && message.Text.Split(' ').First().ToLower() == answersPerStepDictionary[currentStep])
                    {
                        await Bot.SendTextMessageAsync(chatId,
                            "Да, да, именно этот номер.");

                        await Bot.SendTextMessageAsync(chatId,
                            "Санта сказал, что мне нужно отправить письмо на email службы доставки.");

                        await Task.Delay(3000);
                      
                        await Bot.SendTextMessageAsync(chatId,
                            "Чтобы помнить email, я его записываю на жвачке, которую ношу с собой.");

                        await Bot.SendTextMessageAsync(chatId,
                            "Правда в последний раз я положил её в шапку, но так и не могу найти...");

                        await Task.Delay(3000);
                        await Bot.SendTextMessageAsync(chatId,
                            "В теме письма нужно указать место назначения подарка.");

                        // Send photo
                        using (
                            var fileStream = new FileStream(filePathPerStepDictionary[currentStep], FileMode.Open,
                                FileAccess.Read, FileShare.Read))
                        {
                            await Bot.SendPhotoAsync(chatId, fileStream, "");
                        }
                        await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
                        await Task.Delay(3000);
                        await Bot.SendTextMessageAsync(chatId,
                           "1.	Фамилия шеф-повара, который готовит самые вкусные шашлыки.");
                        await Bot.SendTextMessageAsync(chatId,
                           "2.	Главный любитель сыра и конференций.");
                        await Bot.SendTextMessageAsync(chatId,
                           "3.	У этого человека уже подборка афиш грузинских балетов.");
                        await Bot.SendTextMessageAsync(chatId,
                           "4.	Может без проблем в темноте разложить радугу на полутона.");
                        await Bot.SendTextMessageAsync(chatId,
                           "5.	Недавно подключена в матрицу.");
                        await Bot.SendTextMessageAsync(chatId,
                           "6. Этот человек точно знает кто это:");

                        // Send photo
                        using (
                            var fileStream = new FileStream(actorPhoto, FileMode.Open,
                                FileAccess.Read, FileShare.Read))
                        {
                            await Bot.SendPhotoAsync(chatId, fileStream, "");
                        }

                        await Bot.SendTextMessageAsync(chatId,
                           "7.	Среди Happy People у неё больше всего подписчиков в инстаграмме.");
                        // Change step 
                        currentStepPerUserDictionary[chatId] = QuestSteps.Selfie;
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(chatId,
                            "Нет, это точно какая-то ерунда");
                    }
                    break;

                case QuestSteps.Selfie:
                    if (message.Type == MessageType.Photo)
                    {
                        await Bot.SendTextMessageAsync(chatId,
                            "Спасибо, что помогли мне! Счастливого Нового Года!");

                        // Send photo
                        using (
                            var fileStream = new FileStream(elfOfTheYearPhoto, FileMode.Open, FileAccess.Read,
                                FileShare.Read))
                        {
                            await Bot.SendPhotoAsync(chatId, fileStream, "");
                        }
                        await Task.Delay(3000);
                        await Bot.SendTextMessageAsync(chatId,
                            "Я хочу отблагодарить вас");

                        // Send photo
                        using (
                            var fileStream = new FileStream(filePathPerStepDictionary[currentStep], FileMode.Open,
                                FileAccess.Read, FileShare.Read))
                        {
                            await Bot.SendPhotoAsync(chatId, fileStream, "");
                        }
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(chatId,
                            "Нет нет нет, я жду чего-то другого");
                    }
                    break;

                default:
                    await Bot.SendTextMessageAsync(chatId,
                        "Вы что-то делаете не так");
                    return;
            }

        }

        /// <summary>
        /// String message received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="messageEventArgs"></param>
        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                if (message == null)
                {
                    return;
                }

                var userFirstName = message.From.FirstName;
                var userLastName = message.From.LastName;
                var chatId = message.Chat.Id;

                if (message.Text != null)
                {
                    Logger.Log.Info(String.Format("Message received from: {0} {1} text is {2}", userFirstName, userLastName, message.Text.Split(' ').First()));
                    // If received start and it`s new game - add this chat id to dict with start step
                    if (message.Text.Split(' ').First() == "/start" && !currentStepPerUserDictionary.ContainsKey(chatId))
                    {
                        currentStepPerUserDictionary.Add(chatId, QuestSteps.Greeting);
                    }
                }

                QuestSteps userCurrentStep;
                if (currentStepPerUserDictionary.TryGetValue(chatId, out userCurrentStep))
                {
                    SendMessageDependingOnCurrentStep(userCurrentStep, chatId, message);
                }
                else
                {
                    // If in some case user hasn`t started yet
                    currentStepPerUserDictionary.Add(chatId, QuestSteps.Greeting);
                    SendMessageDependingOnCurrentStep(QuestSteps.Greeting, chatId, message);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Error in BotOnMessageReceived", e);
            }
        }

        private static void BotOnChosenInlineResultReceived(object sender,
            ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Logger.Log.Info($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Logger.Log.Error(String.Format("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message));
        }
    }
}

