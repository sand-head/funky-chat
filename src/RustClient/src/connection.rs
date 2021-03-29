use std::{io::Read, net::TcpStream, thread};

use anyhow::Result;
use bytes::Bytes;
use prost::Message;

use crate::{events::{Event, EventHandler}, messages::Response};

pub fn connect(addr: &str, event: &EventHandler) -> Result<()> {
  let mut client = TcpStream::connect(addr)?;
  let tx = event.clone_sender();

  thread::spawn(move || {
    loop {
      let mut data: Vec<u8> = Vec::new();
      // todo: read_to_end waits for the stream to actually stop
      // create a function to pull from stream until message is done
      if let Ok(_) = client.read_to_end(&mut data) {
        let data = Bytes::from(data);
        let response = Response::decode(data)
          .expect("Could not decode response from server");
        if let Some(response) = response.response {
          tx.send(Event::ServerResponse(response))
            .expect("Could not send response event");
        }
      } else {
        eprintln!("Could not read message from server");
        break;
      }
    }
  });

  Ok(())
}
