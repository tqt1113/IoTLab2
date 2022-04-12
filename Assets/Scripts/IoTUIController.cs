using System;
using System.Collections;
using System.Collections.Generic;
using M2MqttUnity;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt.Messages;
public class Status
{
    public float temperature;
    public float humidity;

    public Status(float temperature, float humidity)
    {
        this.humidity = humidity;
        this.temperature = temperature;
    }
}
public class OnOffDeviceStatus
{
    public string device;
    public string status;
    public OnOffDeviceStatus(string device, bool enable)
    {
        this.device = device;
        this.status = enable?"ON":"OFF";
    }

    public bool isOn => status == "ON";
}

public class IoTUIController : M2MqttUnityClient
{
    [SerializeField] private InputField brokerURInput;
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private Button connectBtn;
    [SerializeField] private CanvasGroupUIController authUIController;
    [SerializeField] private CanvasGroupUIController dashboardUIController;
    [SerializeField] private CanvasGroupUIController errorUIController; 
    [SerializeField] private Text errorText;
    [SerializeField] private SliderToggle pumpToggle, ledToggle;
    [SerializeField] private GaugeController humidityController, temperatureController;
    [SerializeField] private bool autoTest = true;

    private const string STATUS_TOPIC = "/bkiot/1814385/status";
    private const string LED_TOPIC = "/bkiot/1814385/led";
    private const string PUMP_TOPIC = "/bkiot/1814385/pump";

    protected override void Awake()
    {
        base.Awake();
        connectBtn.onClick.AddListener(()=>{});
        brokerURInput.onValueChanged.AddListener(OnBrokerAddressChanged);
        usernameInput.onValueChanged.AddListener(OnUsernameChanged);
        passwordInput.onValueChanged.AddListener(OnPasswordChanged);
        pumpToggle.onValueChanged.AddListener(OnPumpControllValueChanged);
        ledToggle.onValueChanged.AddListener(OnLedControllValueChanged);
        connectBtn.onClick.AddListener(OnConnectClick);
    }

    private void OnPumpControllValueChanged(bool enable)
    {
        client.Publish(
            PUMP_TOPIC, 
            System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new OnOffDeviceStatus("PUMP", enable))), 
            MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, 
            false);
    }

    private void OnLedControllValueChanged(bool enable)
    {
        client.Publish(
            LED_TOPIC, 
            System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new OnOffDeviceStatus("LED", enable))), 
            MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, 
            false);
    }

    private void OnConnectClick()
    {
        Connect();
    }
    protected override void OnConnecting()
    {
        base.OnConnecting();
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        authUIController.Hide();
        dashboardUIController.Show();
        errorUIController.Hide();
        if(autoTest)
            AutoTest();
    }

    private void AutoTest()
    {
        OnPumpControllValueChanged(true);
        OnLedControllValueChanged(false);
        client.Publish(
            STATUS_TOPIC, 
            System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Status(50, 50))), 
            MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, 
            true);
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        base.OnConnectionFailed(errorMessage);
        errorText.text = errorMessage;
        errorUIController.Show();
    }

    private void OnPasswordChanged(string newPassword)
    {
        this.mqttPassword = newPassword;
    }

    private void OnUsernameChanged(string newUsername)
    {
        this.mqttUserName = newUsername;
    }

    private void OnBrokerAddressChanged(string newAddress)
    {
        var address = newAddress.Split(':')[0];
        var port = newAddress.Split(':')[1];
        this.brokerAddress = address;
        int.TryParse(port, out this.brokerPort);
    }
    private void OnDestroy() {
        Disconnect();
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { STATUS_TOPIC, LED_TOPIC, PUMP_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE});
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { STATUS_TOPIC, LED_TOPIC, PUMP_TOPIC });
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("Received: " + msg);
        if (topic == STATUS_TOPIC)
            ProcessStatusTopic(msg);
        if (topic == LED_TOPIC)
            ProcessLedTopic(msg);
        if (topic == PUMP_TOPIC)
            ProcessPumpTopic(msg);
    }

    private void ProcessPumpTopic(string msg)
    {
        var pumpStatus = JsonUtility.FromJson<OnOffDeviceStatus>(msg);
        mainThreadQueue.Enqueue(()=> pumpToggle.SetToggleIsOnDontNotify(pumpStatus.isOn));
    }

    private void ProcessLedTopic(string msg)
    {
        var ledStatus = JsonUtility.FromJson<OnOffDeviceStatus>(msg);
        mainThreadQueue.Enqueue(()=> ledToggle.SetToggleIsOnDontNotify(ledStatus.isOn));
    }

    private void ProcessStatusTopic(string msg)
    {
        var systemStatus = JsonUtility.FromJson<Status>(msg);
        mainThreadQueue.Enqueue(()=>{
            humidityController.SetValue(Mathf.Clamp01(Mathf.InverseLerp(0, 100, systemStatus.humidity)), $"{systemStatus.humidity}%");
            temperatureController.SetValue(Mathf.Clamp01(Mathf.InverseLerp(0, 100, systemStatus.temperature)), $"{systemStatus.temperature}<sup>o</sup>C");
        });
    }

    private Queue<Action> mainThreadQueue = new Queue<Action>();
    protected override void Update() {
        base.Update();
        for (int i = 0; i < 100; i++)
        {
            if(mainThreadQueue.Count == 0)
                break;
            mainThreadQueue.Dequeue().Invoke();
        }
    }
}
