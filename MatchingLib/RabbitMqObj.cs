using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    public abstract class RabbitMqBase
    {
        protected virtual ConnectionFactory factory { get; } = new ConnectionFactory();
        protected virtual IConnection conn { get; set; }
        protected virtual IModel channel { get; set; }
        protected virtual string queueName { get; set; }
        protected virtual EventingBasicConsumer consumer { get; set; }
        public void Shutdown()
        {
            if (channel != null) channel.Close();
            if (conn != null) conn.Close();
        }
    }
    public class RabbitMqOut : RabbitMqBase
    {
        public RabbitMqOut(string uri, string queue_name)
        {
            factory.Uri = new Uri(uri);
            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            queueName = queue_name;
        }

        public void Enqueue(byte[] data)
        {
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: data);
        }

        public void Enqueue(IBinaryProcess BinObjIn)
        {
            var binObj = BinObjIn.ToBytes();
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: binObj.bytes);
            BinaryObjPool.Checkin(binObj);
        }
    }
    public class RabbitMqIn : RabbitMqBase
    {
        public RabbitMqIn(string uri, string queue_name)
        {
            factory.Uri = new Uri(uri);
            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            queueName = queue_name;
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    var body = ea.Body;
            //    var message = Encoding.UTF8.GetString(body);
            //    Console.WriteLine(" [x] Received {0}", message);
            //};
        }

        public void BindReceived(EventHandler<BasicDeliverEventArgs> handler)
        {
            consumer.Received += handler;
            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }

        public void MsgFinished(BasicDeliverEventArgs ea)
        {
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
    }
}
