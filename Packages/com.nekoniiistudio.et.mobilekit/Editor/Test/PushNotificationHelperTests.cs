using System;
using System.Reflection;
using NUnit.Framework;
using ET.NotificationSystem;
using UnityEngine;

public class PushNotificationHelperTests
{
    private PushNotificationHelper _helper;
    private PushNotificationData _data;

    [SetUp]
    public void Setup()
    {
        _helper = new PushNotificationHelper();
        _data = ScriptableObject.CreateInstance<PushNotificationData>();
        _data.D7InactivityHours = 168;
        _data.D14InactivityHours = 336;
        _data.D30InactivityHours = 720;
        _data.defaultD7ReminderMessage = "default message";
        _data.notificationItems = new PostGameNotificationItem[]
        {
            new PostGameNotificationItem { timeInHour = 24,  messeger = "24h message" },
            new PostGameNotificationItem { timeInHour = 168, messeger = "D7 item message" },
            new PostGameNotificationItem { timeInHour = 336, messeger = "D14 item message" },
            new PostGameNotificationItem { timeInHour = 720, messeger = "D30 item message" },
        };
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(_data);
    }

    // --- Init ---

    [Test]
    public void Init_StoresData()
    {
        _helper.Init(_data);

        var field = typeof(PushNotificationHelper).GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance);
        var stored = field.GetValue(_helper);
        Assert.AreSame(_data, stored);
    }

    [Test]
    public void Init_StoresCustomMessegerFunc()
    {
        Func<int, string> func = hours => $"custom {hours}h";
        _helper.Init(_data, func);

        var field = typeof(PushNotificationHelper).GetField("_customMessegerFunc", BindingFlags.NonPublic | BindingFlags.Instance);
        var stored = field.GetValue(_helper);
        Assert.AreSame(func, stored);
    }

    [Test]
    public void Init_CustomMessegerFunc_DefaultsToNull()
    {
        _helper.Init(_data);

        var field = typeof(PushNotificationHelper).GetField("_customMessegerFunc", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNull(field.GetValue(_helper));
    }

    // --- GetMessage ---

    [Test]
    public void GetMessage_ReturnsDefaultMessage_WhenNoCustomFunc()
    {
        _helper.Init(_data);
        var result = InvokeGetMessage("default message", 24);
        Assert.AreEqual("default message", result);
    }

    [Test]
    public void GetMessage_ReturnsCustomMessage_WhenFuncReturnsNonEmpty()
    {
        _helper.Init(_data, hours => $"custom {hours}h");
        var result = InvokeGetMessage("default message", 24);
        Assert.AreEqual("custom 24h", result);
    }

    [Test]
    public void GetMessage_ReturnsDefaultMessage_WhenFuncReturnsNull()
    {
        _helper.Init(_data, hours => null);
        var result = InvokeGetMessage("default message", 24);
        Assert.AreEqual("default message", result);
    }

    [Test]
    public void GetMessage_ReturnsDefaultMessage_WhenFuncReturnsEmpty()
    {
        _helper.Init(_data, hours => string.Empty);
        var result = InvokeGetMessage("default message", 24);
        Assert.AreEqual("default message", result);
    }

    [Test]
    public void GetMessage_PassesCorrectHoursToCustomFunc()
    {
        int capturedHours = -1;
        _helper.Init(_data, hours => { capturedHours = hours; return "msg"; });
        InvokeGetMessage("default", 48);
        Assert.AreEqual(48, capturedHours);
    }

    // --- IsD7/D14/D30 NotificationItem ---

    [Test]
    public void IsD7NotificationItem_ReturnsTrueForD7Hours()
    {
        _helper.Init(_data);
        var item = new PostGameNotificationItem { timeInHour = 168 };
        Assert.IsTrue(InvokeIsNotificationItem("IsD7NotificationItem", item));
    }

    [Test]
    public void IsD7NotificationItem_ReturnsFalseForNonD7Hours()
    {
        _helper.Init(_data);
        var item = new PostGameNotificationItem { timeInHour = 24 };
        Assert.IsFalse(InvokeIsNotificationItem("IsD7NotificationItem", item));
    }

    [Test]
    public void IsD14NotificationItem_ReturnsTrueForD14Hours()
    {
        _helper.Init(_data);
        var item = new PostGameNotificationItem { timeInHour = 336 };
        Assert.IsTrue(InvokeIsNotificationItem("IsD14NotificationItem", item));
    }

    [Test]
    public void IsD14NotificationItem_ReturnsFalseForNonD14Hours()
    {
        _helper.Init(_data);
        var item = new PostGameNotificationItem { timeInHour = 24 };
        Assert.IsFalse(InvokeIsNotificationItem("IsD14NotificationItem", item));
    }

    [Test]
    public void IsD30NotificationItem_ReturnsTrueForD30Hours()
    {
        _helper.Init(_data);
        var item = new PostGameNotificationItem { timeInHour = 720 };
        Assert.IsTrue(InvokeIsNotificationItem("IsD30NotificationItem", item));
    }

    [Test]
    public void IsD30NotificationItem_ReturnsFalseForNonD30Hours()
    {
        _helper.Init(_data);
        var item = new PostGameNotificationItem { timeInHour = 24 };
        Assert.IsFalse(InvokeIsNotificationItem("IsD30NotificationItem", item));
    }

    // --- Helpers ---

    private string InvokeGetMessage(string defaultMessage, int hoursFromNow)
    {
        var method = typeof(PushNotificationHelper).GetMethod(
            "GetMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        return (string)method.Invoke(_helper, new object[] { defaultMessage, hoursFromNow });
    }

    private bool InvokeIsNotificationItem(string methodName, PostGameNotificationItem item)
    {
        var method = typeof(PushNotificationHelper).GetMethod(
            methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (bool)method.Invoke(_helper, new object[] { item });
    }
}
