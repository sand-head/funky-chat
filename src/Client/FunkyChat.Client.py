
import socket
import tkinter as tk
import Messages_pb2 as mpb

HOST, PORT = '127.0.0.1', 13337
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

def connect():
    sock.connect((HOST, PORT))
    
    # receive welcome response from server
    response = mpb.Response()
    data = sock.recv(1024)
    response.ParseFromString(data)
    print('Welcome, ', response.welcome.user_id, "!", sep="")
    online_users = ", ".join(response.welcome.connected_users) \
        if len(response.welcome.connected_users) > 0 \
        else "None"
    print('Online:', online_users)
    
    # command = mpb.Command()
    # command.echo.message = 'welcome'
    # sock.send(command.SerializeToString())

    while True:
        user_input = input("> ")

        # parse command from input
        command = mpb.Command()
        if user_input.startswith(".exit"):
            command.exit.SetInParent()
            sock.send(command.SerializeToString())
            break

        command.echo.message = user_input

        # send command to server
        sock.send(command.SerializeToString())

        # read response from server
        response = mpb.Response()
        response.ParseFromString(sock.recv(2048))
        print("<", response.echo.message)

connect()