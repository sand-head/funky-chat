use std::{
  sync::mpsc,
  thread::{self, JoinHandle},
};

use crossterm::event::{self, Event, KeyEvent};

pub struct Input {
  rx: mpsc::Receiver<KeyEvent>,
  input_handle: JoinHandle<()>,
}
impl Default for Input {
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
            if let Event::Key(key_event) = event {
              if let Err(e) = tx.send(key_event) {
                // on error, print and break the loop
                eprintln!("{}", e);
                break;
              }
            }
          }
        }
      })
    };

    Input { rx, input_handle }
  }
}
impl Input {
  /// Receives the next keypress made by the user
  pub fn next(&self) -> Result<KeyEvent, mpsc::RecvError> {
    self.rx.recv()
  }
}
