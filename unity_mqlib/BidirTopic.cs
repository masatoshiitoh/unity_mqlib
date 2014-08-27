// This source code is licensed under the Apache License, version 2.0.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2013 GoPivotal, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//

using UnityEngine;

using System.Collections;
using System.Threading;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;


public class BidirTopic : MonoBehaviour {
	public string serverip;
	public string ToClientEx;
	
	ConnectionFactory cf;
	IConnection conn;
	IModel ch = null;
	QueueingBasicConsumer consumer;
	
	System.Collections.Queue queue = null;
	string lastMessage;
	
	// Use this for initialization
	void Start()
	{
		cf = new ConnectionFactory();
		cf.HostName = serverip;
		conn = cf.CreateConnection();
		
		conn.ConnectionShutdown += new ConnectionShutdownEventHandler(LogConnClose);
		
		ch = conn.CreateModel();
		ch.ExchangeDeclare(ToClientEx, "topic",false,true,null);
		string queueName = ch.QueueDeclare();
		
		ch.QueueBind(queueName, ToClientEx, "#");
		consumer = new QueueingBasicConsumer(ch);
		ch.BasicConsume(queueName, true, consumer);
		
		queue = new System.Collections.Queue();
		queue.Clear();
	}
	
	// Update is called once per frame
	void Update()
	{
		if (ch == null || consumer == null) return;
		BasicDeliverEventArgs ea;
		while ((ea = (BasicDeliverEventArgs)consumer.Queue.DequeueNoWait(null)) != null)
		{
			var body = ea.Body;
			var routingKey = ea.RoutingKey;
			var message = Encoding.UTF8.GetString(body);
			queue.Enqueue(message + " from " + routingKey);
		}
	}
	
	void OnGUI()
	{
		if (queue != null && queue.Count > 0)
		{
			lastMessage = queue.Dequeue().ToString();
		}
		GUILayout.Label((string)lastMessage);
	}
	
	public static void LogConnClose(IConnection conn, ShutdownEventArgs reason)
	{
		Debug.Log("Closing connection normally. " + conn + " with reason " + reason);
	}
}
