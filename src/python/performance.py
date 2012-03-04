#!/usr/bin/env python
import pika
import time

# config
print_output = False
durable = False
ack = False
num_msgs = 100000
receive = False
broker = "10.0.1.2"
override_queue = "Intersystem"

print "Starting..."
start = time.time()

# Initiate a connection
print "Connecting to %s..." % broker
params = pika.ConnectionParameters(broker) #my home PC
connection = pika.BlockingConnection(params)
channel = connection.channel()

def if_not(boolval): "" if boolval else "NOT"

print "Durability is %s enabled" % if_not(durable)
print "Ack is %s enabled" % if_not(ack)

# Declare the queue first
queue_name = "PythonPublish%s" % ("Durable" if durable else "NotDurable")
if(override_queue != ''):
	queue_name = override_queue

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

if receive:
	def on_receive(ch, method, properties, body):
	 	if print_output:
	 		print "Received %r %r" % (body, method.delivery_tag)
	 	
	 	# ACK the receipt of the message
	 	if ack:
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