#!/usr/bin/env python
import pika
import datetime

broker = "10.0.1.2"
today = datetime.datetime.now()

# Initiate a connection
print "Connecting to %s..." % broker
params = pika.ConnectionParameters(broker) #my home PC
connection = pika.BlockingConnection(params)
channel = connection.channel()

queue_name = "IveGotAJobForYou"

# Declare the queue (durable)
channel.queue_declare(queue = queue_name, durable = True)

# Define properties (delivery_mode 2 is durable)
properties = pika.BasicProperties(delivery_mode = 2)

# Publish a message!
channel.basic_publish(exchange = "", 
	routing_key = queue_name, 
	body = "Hello World in %s!" % today,
	properties = properties)

print "Message Sent!"

# Define receive callback
def on_receive(ch, method, properties, body):
	print "Received '%s'" % body
	ch.basic_ack(delivery_tag = method.delivery_tag)

# Receive the messages!
while connection.is_open:
	method_frame, header_frame, body = channel.basic_get(queue = queue_name)	

	# check the method frame for an empty result
	if method_frame.NAME != 'Basic.GetEmpty':
		on_receive(channel, method_frame, None, body)
	else:
		break

# All done!
connection.close()