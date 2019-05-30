using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;


// Display an information in front of the user (for a amount of time)
public class NotifyManager : SingletonCustom<NotifyManager>
{
    

    public class Notify
    {
        public string m_MessageText;
        public NotifyType m_NotifyType;
        public int m_DisplaySeconds = 6;

         public Notify(NotifyType pType, string pMessage)
         {
            m_MessageText = pMessage;
            m_NotifyType = pType;
         }
    }

    public enum NotifyType
    {
        Error,
        Info
    }

    private ConcurrentQueue<Notify> m_Notifications = new ConcurrentQueue<Notify>();

    public void AddMessage(NotifyType pType, string pMessage)
    {
        m_Notifications.Enqueue(new Notify(pType, pMessage));
    }

    public Notify GetNotifyMessage()
    {
        if (m_Notifications.IsEmpty) return null;
        Notify msg = null;
        if (m_Notifications.TryDequeue(out msg)) return msg;
        return null; 
    }





}
