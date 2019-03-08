using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotCore.Manager
{
    public class BotManager
    {
        public readonly TelegramBotClient Bot;
        private readonly Messages questions;
        private ReplyKeyboardMarkup replyKeyboard;
        private readonly IRepository repository;
        private List<Question> questionTempList;
        private int questionLimit;

        public BotManager(IRepository _repository, string botToken, string messageJsonPath, int _questionLimit)
        {
            Bot = new TelegramBotClient(botToken);
            questions = JsonConvert.DeserializeObject<Messages>(File.ReadAllText(messageJsonPath));
            repository = _repository;
            questionTempList = new List<Question>();
            questionLimit = _questionLimit;
        }

        public async void AskQuestion(List<string> questionsList, long chatId, string subject)
        {
            try
            {
                await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
                await Task.Delay(500);

                replyKeyboard = new[]
                {
               questionsList.ToArray()
            };

                replyKeyboard.ResizeKeyboard = true;
                replyKeyboard.OneTimeKeyboard = true;

                await Bot.SendTextMessageAsync(
                chatId,
                subject,
                replyMarkup: replyKeyboard);
            }
            catch (Exception ex)
            {
                //Some Exception logging process..
            }
        }

        public async void SendTextMessage(string message, long chatId)
        {
            try
            {
                await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
                await Task.Delay(500);

                await Bot.SendTextMessageAsync(
                    chatId: chatId,
                    text: message
                );
            }
            catch (Exception ex)
            {
                //Some Exception logging process..
            }
        }

        public void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            var userStepTemp = repository.GetStep(message.Chat.Id);

            // If anyone reach daily limit , cannot ask question
            if (!repository.CheckQuestionLimit(message.Chat.Id, questionLimit))
                SendTextMessage(questions.Information.GetValueOrDefault("ReachedDailyLimit"), message.Chat.Id);

            else
            {
                var user = repository.GetUser(message.Chat.Id);
                if (user == null)
                {
                    user = new User { UserId = message.Chat.Id, Username = message.Chat.Username, Firstname = message.Chat.FirstName, Lastname = message.Chat.LastName };
                    repository.SaveUser(user);
                }


                var userQuestion = questionTempList.FirstOrDefault(x => x.User.UserId == message.Chat.Id);
                if (userQuestion == null)
                    userQuestion = new Question() { User = user };

                if (userStepTemp == null)
                    repository.SaveUpdateStep(message.Chat.Id, 0);

                if (message == null || message.Type != MessageType.Text) return;

                switch (userStepTemp == null ? 0 : userStepTemp.QuestionStep)
                {
                    case 0: // Welcome and description
                        SendTextMessage(questions.Information.GetValueOrDefault("Welcome"), message.Chat.Id);
                        SendTextMessage(questions.QuestionText.GetValueOrDefault("Description"), message.Chat.Id);
                        userQuestion.Timestamp = DateTime.Now;
                        userQuestion.User = user;
                        AddQuestionTempList(userQuestion);
                        repository.SaveUpdateStep(message.Chat.Id, 1);
                        break;
                    case 1: //Priority
                        userQuestion.Description = message.Text;
                        AddQuestionTempList(userQuestion);
                        AskQuestion(questions.Priority, message.Chat.Id, questions.QuestionText.GetValueOrDefault("Priority"));
                        repository.SaveUpdateStep(message.Chat.Id, 2);
                        break;
                    case 2: //Email //TODO, it cannot run not yet
                        userQuestion.Priority = message.Text;
                        AddQuestionTempList(userQuestion);
                        AskQuestion(questions.Email, message.Chat.Id, questions.QuestionText.GetValueOrDefault("Email"));
                        repository.SaveUpdateStep(message.Chat.Id, 3);
                        break;
                    case 3: //Success
                        userQuestion.EmailSending = message.Text == "Evet" ? true : false;
                        AddQuestionTempList(userQuestion);
                        SendTextMessage(questions.Information.GetValueOrDefault("Success"), message.Chat.Id);
                        repository.SaveQuestion(userQuestion);
                        repository.DeleteStep(message.Chat.Id);
                        DeleteQuestionTempList(userQuestion);
                        break;
                    default:
                        SendTextMessage(questions.Information.GetValueOrDefault("Error"), message.Chat.Id);
                        repository.DeleteStep(message.Chat.Id);
                        break;
                }
            }
        }

        // Add or Update QuestionTemp, For each steps
        public void AddQuestionTempList(Question question)
        {
            var questionTemp = questionTempList.FirstOrDefault(x => x.User.UserId == question.User.UserId);
            if (questionTemp == null)
            {
                questionTempList.Add(question);
            }
            else
            {
                var index = questionTempList.IndexOf(questionTemp);
                questionTempList[index] = question;
            }
        }

        // If all steps finish, delete temp
        public void DeleteQuestionTempList(Question question)
        {
            var questionTemp = questionTempList.FirstOrDefault(x => x.User.UserId == question.User.UserId);
            if (questionTemp != null)
                questionTempList.Remove(question);
        }

        public void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}
