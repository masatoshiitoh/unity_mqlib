using UnityEngine;

using System.Collections;
using System.Threading;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;

public class MqSubscriber : MonoBehaviour {
    public string serverip;
    public string exchange;
    
    ConnectionFactory cf;
    IConnection conn;
    IModel ch = null;
    QueueingBasicConsumer consumer;

    System.Collections.Queue queue = null;
    string lastMessage;

	// Use this for initialization
	void Start () {
        cf = new ConnectionFactory();
        cf.HostName = serverip;
        conn = cf.CreateConnection();

        conn.ConnectionShutdown += new ConnectionShutdownEventHandler(LogConnClose);

        ch = conn.CreateModel();
        ch.ExchangeDeclare(exchange, "fanout");
        string queueName = ch.QueueDeclare();

        ch.QueueBind(queueName, exchange, "");
        consumer = new QueueingBasicConsumer(ch);
        ch.BasicConsume(queueName, true, consumer);

        queue = new System.Collections.Queue();
        queue.Clear();
	}
	
	// Update is called once per frame
	void Update () {
        if (ch == null) return;
        BasicDeliverEventArgs ea;
        while ((ea = (BasicDeliverEventArgs)consumer.Queue.DequeueNoWait(null)) != null)
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);
            queue.Enqueue(message);
        }
	}
    
    public static void LogConnClose(IConnection conn, ShutdownEventArgs reason)
    {
    }
}
