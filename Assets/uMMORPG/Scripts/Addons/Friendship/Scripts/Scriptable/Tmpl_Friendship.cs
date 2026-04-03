using UnityEngine;

// FRIENDLIST
[CreateAssetMenu(fileName = "Friendship", menuName = "ADDON/Templates/New Friendship", order = 999)]
public class Tmpl_Friendship : ScriptableObject
{

    [Header("[-=-[ Game Event ]-=-]")]
    public GameEvent GameEventFriendship;
    public GameEvent GameEventFriendInvite;
    public GameEvent GameEventCoupleInvite;

    [Header("[-=-[ Friendship ]-=-]")]
    public int maxFriends = 50;
    public float inviteFriendWaitSeconds = 3;

    [Header("[-=-[ Couple & Wedding ]-=-]")]
    [Tooltip("if true, the couple system will be activated and players will have to be friends for at least minDayFriend before being able to get coupled")]
    public bool allowCouple = true;

    [BoolShowConditional(conditionFieldName: "allowCouple", conditionValue: true)]
    [Min(0)] public int minDayFriend = 15;

    [BoolShowConditional(conditionFieldName: "allowCouple", conditionValue: true)]
    [Tooltip("if true, the wedding will be automatically canceled if one of the 2 players is offline for more than maxDayOffline")]
    public bool autoDivorce = true;

    [BoolShowConditional(conditionFieldName: "allowCouple", conditionValue: true)]
    [Min(0)] public int maxDayOffline = 90;
    [BoolShowConditional(conditionFieldName: "allowCouple", conditionValue: true)]
    [Tooltip("this is directly in percent int exemple : 10 = 10%, 50 = 50%")]
    [Min(0)] public int increaseRewardForCouple = 20;

    [BoolShowConditional(conditionFieldName: "allowCouple", conditionValue: true)]
    [Tooltip("this cost is paid by the 2 players who get coupled")]
    public Tools_Cost coupleCostPerCharacter;

    [Header("[-=-[ Gifts reward ]-=-]")]
    public bool allowGifts = true;

    [BoolShowConditional(conditionFieldName: "allowGifts", conditionValue: true)]
    [Tooltip("[-=-[ Gift Add Points ]-=-]")]
    [Min(0)] public int rewardPoint;

    [BoolShowConditional(conditionFieldName: "allowGifts", conditionValue: true)]
    [Tooltip("[-=-[ Hugs Reward Character ]-=-]")]
    public Tools_Reward rewardCharacter;
    [BoolShowConditional(conditionFieldName: "allowGifts", conditionValue: true)]
    [Tooltip("[-=-[ Hugs Reward Friend ]-=-]")]
    public Tools_Reward rewardFriend;

    [BoolShowConditional(conditionFieldName: "allowGifts", conditionValue: true)]
    [Tooltip("How many hours must pass between hugs?")]
    public int timespanHours;

    [Header("[-=-[ Display Message ]-=-]")]
    public bool editMessage = false;
    #region message
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgMaxFriends = "You cannot have anymore friends!";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgIsNowFriend = "{0} is now your friend.";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgIsAlreadyFriend = "{0} is already your friend";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgCannotFriendSelf = "You cannot add yourself as a friend.";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgTargetOffline = "{0} is not online.";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgTargetNotExist = "A player {0} does not exist.";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgNoLongerFriend = "{0} is no longer your friend.";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgTargetNotFriend = "{0} is not your friend.";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgGiftedFriend = "You Gifted {0}.";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgGiftedFriendBy = "You received a gift from {0}.";

    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgCongrulationCoupled = "Congratulations you are now in couple with {0}";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgAlreadyCoupled = "Sorry you or your friend is already in couple";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgWeddingCostNoOk = "Sorry {0} does not have the necessary for the wedding";

    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgWeddingNumberDayNoOk = "sorry, you must have been friends for at least {0} days!";

#if _iMMOPVP
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgNoSameRealm = "sorry, you must be in the same realm as the person you want to invite!";
#endif

    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleNoOk = "[Couple] You or your friend does not have the necessary for the wedding";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleDay = "[Couple] Friends since : {0} Day(s)";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleStamina = "Couple] stamina Cost : {0} Point(s)";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleHealth = "[Couple] Health Cost :{0} Point(s)";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleCoin = "[Couple] Coin Cost : {0} coin(s)";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleExperience = "[Couple] Experience Cost : {0} Point(s)";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleGold = "[Couple] Gold Cost : {0} Point(s)";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleMana = "[Couple] Mana Cost : {0} Point(s)";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleSkillExperience = "[Couple] Skill Experience Cost : {0} Point(s)";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleHonnorCurrency = "[Couple] Honnor Currency cost : {0} {1}";
    [BoolShowConditional(conditionFieldName: "editMessage", conditionValue: true)]
    public string msgRequirementCoupleItem = "[Couple] Item Cost : {0} {1}";
    #endregion message
}