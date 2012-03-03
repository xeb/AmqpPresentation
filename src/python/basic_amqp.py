#!/usr/bin/env python
import pika
import time

# config
print_output = False
durable = False
num_msgs = 1000
broker = "127.0.0.1"

print "Starting..."
start = time.time()

print "Durability is %s" % ("enabled" if durable else "NOT enabled")

# Initiate a connection
print "Connecting to %s..." % broker
params = pika.ConnectionParameters(broker) #my home PC
connection = pika.BlockingConnection(params)
channel = connection.channel()

# Declare the queue first
queue_name = "PythonPublish%s" % ("Durable" if durable else "NotDurable")
print "Declaring queue '%s'..." % queue_name
channel.queue_declare(queue = queue_name, durable = durable)

# Publish a message
# ALWAYS pass an exchange -- even an empty string
print "Publishing %s messages..." % num_msgs
properties = pika.BasicProperties(content_type="text/plain", delivery_mode= 2 if durable else 1)
for i in range(num_msgs):
 	channel.basic_publish(exchange = "", routing_key= queue_name, body = "Hello from my Mac!")

# Now receive some messages
msgs = 0

def on_receive(ch, method, properties, body):
 	if print_output:
 		print "Received %r %r" % (body, method.delivery_tag)
 	
 	# ACK the receipt of the message
 	ch.basic_ack(delivery_tag = method.delivery_tag)
 	
print "Receiving all messages..."
while connection.is_open:
	method_frame, header_frame, body = channel.basic_get(queue = queue_name)	

	# check the method frame for an empty result
	if method_frame.NAME != 'Basic.GetEmpty':
		on_receive(channel, method_frame, None, body)
		msgs += 1
	else:
		break

print "Fin %s msgs in %s seconds" % (msgs, time.time() - start)

connection.close()