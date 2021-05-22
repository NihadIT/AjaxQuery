using AJAXJQueryChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AJAXJQueryChat.Controllers
{
    public class HomeController : Controller
    {
        static ChatModel chatModel;
        public ActionResult Index(string user, bool? logOn, bool? logOff, string chatMessage)
        {
            try
            {
                if (chatModel == null) chatModel = new ChatModel();

                if (chatModel.Messages.Count > 100)
                    chatModel.Messages.RemoveRange(0, 90);

                // если обычный запрос, просто возвращаем представление
                if (!Request.IsAjaxRequest())
                {
                    return View(chatModel);
                }

                else if((logOn != null && (bool)logOn)){ 
                if(chatModel.Users.FirstOrDefault(u => u.Name == user) != null)
                    {
                        throw new Exception("Пользователь с таким ником уже существует");
                    }

                    else if (chatModel.Users.Count > 10)
                    {
                        throw new Exception("Чат заполнен");
                    }

                    else
                    {
                        chatModel.Users.Add(new ChatUser()
                        {
                            Name = user,
                            LoginTime = DateTime.Now,
                            LastPing = DateTime.Now
                        });

                        chatModel.Messages.Add(new ChatMessage()
                        {
                            Text = user + " вошел в чат",
                            Date = DateTime.Now
                        });
                    }
                    return PartialView("ChatRoom", chatModel);

                }

                // если передан параметр logOff
                else if (logOff != null && (bool)logOff)
                {
                    LogOff(chatModel.Users.FirstOrDefault(u => u.Name == user));
                    return PartialView("ChatRoom", chatModel);
                }
                else
                {
                    ChatUser currentUser = chatModel.Users.FirstOrDefault(u => u.Name == user);

                    //для каждлого пользователя запоминаем воемя последнего обновления
                    currentUser.LastPing = DateTime.Now;

                    // удаляем неавтивных пользователей, если время простоя больше 15 сек
                    List<ChatUser> toRemove = new List<ChatUser>();
                    foreach (Models.ChatUser usr in chatModel.Users)
                    {
                        TimeSpan span = DateTime.Now - usr.LastPing;
                        if (span.TotalSeconds > 15)
                            toRemove.Add(usr);
                    }
                    foreach (ChatUser u in toRemove)
                    {
                        LogOff(u);
                    }

                    // добавляем в список сообщений новое сообщение
                    if (!string.IsNullOrEmpty(chatMessage))
                    {
                        chatModel.Messages.Add(new ChatMessage()
                        {
                            User = currentUser,
                            Text = chatMessage,
                            Date = DateTime.Now
                        });
                    }

                    return PartialView("History", chatModel);
                }
            }
            catch (Exception ex)
            {
                //в случае ошибки посылаем статусный код 500
                Response.StatusCode = 500;
                return Content(ex.Message);
            }

            
        }
        // при выходе пользователя удаляем его из списка
        public void LogOff(ChatUser user)
        {
            chatModel.Users.Remove(user);
            chatModel.Messages.Add(new ChatMessage()
            {
                Text = user.Name + " покинул чат.",
                Date = DateTime.Now
            });
        }
    }

    }
