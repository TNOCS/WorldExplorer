using Assets.Scripts.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.Scripts.Plugins;
using HoloToolkit.Unity;

public class SelectionHandler : Singleton<SelectionHandler>
{
    private SessionManager session;

    // Use this for initialization
    private  Dictionary<User, GameObject> selection = new Dictionary<User, GameObject>();
    void Start()
    {
        session = SessionManager.Instance;
    }

    public bool GameObjectIsSelected(GameObject obj)
    {
        return selection.ContainsValue(obj);
    }

    public void releaseObj(User u)
    {
        selection[u] = null;
    }

    public bool UserHasSelection(User u)
    {
        bool r = false;
        foreach (var v in selection)
        {
            if (v.Key.Id != u.Id) continue;
            r = v.Value != null;
        }
        return r;
    }

    public User GetSelectedUser(GameObject obj)
    {
        User u = null;
        foreach (var c in selection)
        {
            if (c.Value == obj)
                u = c.Key;
        }
        return u;
    }

    /// <summary>
    /// Set gameobject as selected  by user however should it already be selected or user has a selected it will return false and not select
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public bool GameObjectSelect(GameObject obj, User user)
    {
        if (selection.Count == 0) addUser(user);
        if (GameObjectIsSelected(obj) || UserHasSelection(user))
            return false;
        if (!selection.ContainsKey(user)) selection.Add(user, obj);
        else
            selection[user] = obj;
        return true;
    }

    public void addUser(User m)
    {
        selection.Add(m, null);
    }

    public GameObject GetSelectedObject(User me)
    {
        return selection[me];
    }

    public void releaseObj()
    {
        if (UserHasSelection(session.me))
            selection[session.me].BroadcastMessage("OnSelect", SendMessageOptions.RequireReceiver);
    }
}
