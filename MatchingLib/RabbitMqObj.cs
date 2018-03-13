using BaseHelper;
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
        public virtual objPool<IModel> channelPool { get; set; }
        protected virtual string queueName { get; set; }
        protected virtual EventingBasicConsumer consumer { get; set; }
        public virtual void Shutdown()
        {
            //if (channel != null) channel.Close();
            if (conn != null) conn.Close();
        }
    }
    public class RabbitMqOut : RabbitMqBase
    {
        public RabbitMqOut(string uri, string queue_name)
        {
            factory.Uri = new Uri(uri);
            conn = factory.CreateConnection();
            channelPool = new objPool<IModel>(() => conn.CreateModel());
            queueName = queue_name;
        }

        public void Enqueue(byte[] data)
        {
            var ch = channelPool.Checkout();
            var properties = ch.CreateBasicProperties();
            //properties.Persistent = true;
            ch.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: data);
            channelPool.Checkin(ch);
        }

        public void Enqueue(IBinaryProcess BinObjIn)
        {
            var binObj = BinObjIn.ToBytes();
            Enqueue(binObj.bytes);
            BinaryObjPool.Checkin(binObj);
        }
    }
    public class RabbitMqIn : RabbitMqBase
    {
        public RabbitMqIn(string uri, string queue_name, ushort prefetchCount = 1)
        {
            factory.Uri = new Uri(uri);
            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            queueName = queue_name;
            //channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: true);
            consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    var body = ea.Body;
            //    var message = Encoding.UTF8.GetString(body);
            //    Console.WriteLine(" [x] Received {0}", message);
            //};
        }

        public void BindReceived(EventHandler<BasicDeliverEventArgs> handler, bool autoAck = true)
        {
            consumer.Received += handler;
            channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
        }

        public void MsgFinished(BasicDeliverEventArgs ea)
        {
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
    }
}
