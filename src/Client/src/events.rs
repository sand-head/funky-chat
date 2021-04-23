use std::{
  sync::mpsc,
  thread::{self, JoinHandle},
};

use crossterm::event::{self, Event as TermEvent, KeyEvent};

use crate::messages::response::Response;

pub enum Event {
  UserInput(KeyEvent),
  Resize(u16, u16),
  ServerResponse(Response),
}

pub struct EventHandler {
  tx: mpsc::Sender<Event>,
  rx: mpsc::Receiver<Event>,
  input_handle: JoinHandle<()>,
}
impl Default for EventHandler {
  fn default() -> Self {
    // open message passing channel
    let (tx, rx) = mpsc::channel();
    let input_handle = {
      let tx = tx.clone();
      thread::spawn(move || {
        loop {
          let event = event::read();
          if let Ok(event) = event {
            // check to make sure we have a key event before transmitting
            match event {
              TermEvent::Key(key_event) => {
                if let Err(e) = tx.send(Event::UserInput(key_event)) {
                  // on error, print and break the loop
                  eprintln!("{}", e);
                  break;
                }
              }
              TermEvent::Resize(x, y) => {
                if let Err(e) = tx.send(Event::Resize(x, y)) {
                  // on error, print and break the loop
                  eprintln!("{}", e);
                  break;
                }
              }
              _ => {}
            }
          }
        }
      })
    };

    EventHandler {
      tx,
      rx,
      input_handle,
    }
  }
}
impl EventHandler {
  /// Receives the next event.
  pub fn next(&self) -> Result<Event, mpsc::RecvError> {
    self.rx.recv()
  }

  /// Clones the sender, allowing it to be used by another thread.
  pub fn clone_sender(&self) -> mpsc::Sender<Event> {
    self.tx.clone()
  }
}
