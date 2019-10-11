using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MyDebug : MonoBehaviour
{
    private static MyDebug Instance;
    private static Queue<Message> messages;
    private static Text[] texts;

    private void Awake()
    {
        Instance = this;

        Initialize();
    }

    private static void Initialize()
    {
        messages = new Queue<Message>();

        var temp = UI.Instance?.debugLog?.transform;

        if (temp == null) return;

        texts = new Text[temp.childCount];
        for (int i = temp.childCount - 1, j = 0; i >= 0; i--, j++)
        {
            texts[j] = temp.GetChild(i).GetComponent<Text>();
        }
    }

    public static void Log(object message)
    {
        AddMessage(Color.white, message);
        if (GameDataManager.data?.debugEnabled ?? false)
        {
            Debug.Log(message);
        }
    }
    public static void LogWarning(object message)
    {
        AddMessage(new Color(1, 0.8f, 0), message);
        if (GameDataManager.data?.debugEnabled ?? false)
        {
            Debug.LogWarning(message);
        }
    }
    public static void LogError(object message)
    {
        AddMessage(Color.red, message);
        if (GameDataManager.data?.debugEnabled ?? false)
        {
            Debug.LogError(message);
        }
    }

    private static void AddMessage(Color color, object message)
    {
        if (messages == null) Initialize();
        messages.Enqueue(new Message(color, message));

        if (texts == null) return;

        if (messages.Count > texts.Length) messages.Dequeue();

        var tempArr = messages.ToArray();

        for (int i = 0, j = messages.Count - 1; i < messages.Count; i++, j--)
        {
            texts[i].color = tempArr[j].color;
            texts[i].text = tempArr[j].text;
        }

        //DeleteMessage();
    }

    private async static void DeleteMessage()
    {
        Debug.Log("Enter method");
        float timer = 7;
        await Task.Run(() =>
        {
            Debug.Log("Starting task");
            while (timer > 0)
            {
                timer -= 0.02f;
                Debug.Log(timer);
            }
            Debug.Log("Finishing task");
        });
        Debug.Log("Continue method");
        if (messages.Count > 0)
        {
            if (messages.Count == 1)
            {
                texts[0].color = Color.white;
                texts[0].text = "Debug";
                messages.Dequeue();
                return;
            }

            if (texts != null && messages != null)
            {
                texts[messages.Count - 1].text = "";
                messages.Dequeue();
            }
        }
    }

    private struct Message
    {
        public string text;
        public Color color;

        public Message(Color color, object message)
        {
            this.color = color;
            text = message?.ToString() ?? "null";
        }
    }
}
