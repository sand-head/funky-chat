import socket
import tkinter as tk
from src.PythonClient import messages_pb2 as mpb

HOST, PORT = '127.0.0.1', 13337
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)


def connect():

    response = mpb.Response()
    command = mpb.Command()

    sock.connect((HOST, PORT))
    data = sock.recv(1024)
    response.ParseFromString(data)
    print('name', response.welcome.user_id)
    command.echo.message = 'welcome'
    sock.send(command.SerializeToString())

    while True:
        continue

connect()

