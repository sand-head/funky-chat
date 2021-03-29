use std::{
  io::{Read, Write},
  net::TcpStream,
  sync::mpsc::{self, SendError, Sender},
  thread::{self, JoinHandle},
};

use anyhow::Result;
use bytes::Bytes;
use prost::Message;

use crate::{
  events::{Event, EventHandler},
  messages::{Command, Response},
};

const BUFFER_SIZE: usize = 2048;

fn read_entire_msg(stream: &mut TcpStream) -> Vec<u8> {
  let mut data: Vec<u8> = Vec::new();
  let mut total_size: usize = 0;

  loop {
    let mut buffer = vec![0; BUFFER_SIZE];
    if let Ok(size) = stream.read(&mut buffer) {
      total_size += size;
      data.append(&mut buffer);

      if size <= BUFFER_SIZE {
        break;
      }
    } else {
      break;
    }
  }

  data[..total_size].to_vec()
}

pub struct Connection {
  tx: Sender<Command>,
  recv_handle: JoinHandle<()>,
  send_handle: JoinHandle<()>,
}
impl Connection {
  pub fn connect(addr: &str, event: &EventHandler) -> Result<Self> {
    let mut client = TcpStream::connect(addr)?;

    // handle incoming responses from server
    let tx = event.clone_sender();
    let mut recv_client = client.try_clone().expect("Could not clone client");
    let recv_handle = thread::spawn(move || {
      loop {
        let data = Bytes::from(read_entire_msg(&mut recv_client));
        let response = Response::decode(data).expect("Could not decode response from server");

        if let None = response.response {
          // no op
          continue;
        } else {
          tx.send(Event::ServerResponse(response.response.unwrap()))
            .expect("Could not send response event");
        }
      }
    });

    // handle outgoing commands to server
    let (tx, rx) = mpsc::channel();
    let send_handle = thread::spawn(move || loop {
      let command: Command = rx.recv().unwrap();
      let mut buffer = Vec::<u8>::new();
      command
        .encode(&mut buffer)
        .expect("Could not encode command to buffer");
      client
        .write(&buffer)
        .expect("Could not write command to server");
    });

    Ok(Connection {
      tx,
      recv_handle,
      send_handle,
    })
  }

  pub fn send(&self, command: Command) -> Result<(), SendError<Command>> {
    self.tx.send(command)
  }
}
