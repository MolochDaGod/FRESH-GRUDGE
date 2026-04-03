// FRIEND
using System;

public partial struct Friend
{
    public string name;
    public string name_requester;
    public string name_accepted;
    public int friendPoint;
    public DateTime created;
    public bool coupled;
    public string lastGiftedRequester;
    public string lastGiftedAccepted;

    public int level;
    public string _class;
    public string guild;
    public bool online;
    public bool inParty;

    // -----------------------------------------------------------------------------------
    // Friend (Constructor)
    // -----------------------------------------------------------------------------------
    public Friend(string _name, string nameRequester, string nameAccepted, int _friendPoint, DateTime _created, bool _coupled, string _lastGiftedRequester, string _lastGiftedAccepted)
    {
        name = _name;
        name_requester = nameRequester;
        name_accepted = nameAccepted;
        friendPoint = _friendPoint;
        created = _created;
        coupled = _coupled;
        lastGiftedRequester = _lastGiftedRequester;
        lastGiftedAccepted = _lastGiftedAccepted;

        level = 0;
        _class = "";
        guild = "";
        online = false;
        inParty = false;
    }

    // -----------------------------------------------------------------------------------
}