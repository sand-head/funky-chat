use app::{App, Message};
use connection::connect;
use crossterm::event;
use events::EventHandler;
use ui::draw_app;

use std::io;

use anyhow::Result;
use tui::{backend::CrosstermBackend, Terminal};

mod app;
mod connection;
mod events;
mod ui;

pub mod messages {
  include!(concat!(env!("OUT_DIR"), "/funkychat.protos.rs"));
}

fn main() -> Result<()> {
  // set up terminal
  let stdout = io::stdout();
  let backend = CrosstermBackend::new(stdout);
  let mut terminal = Terminal::new(backend)?;
  terminal.clear()?;

  // set up application struct
  let mut app = App::default();

  // set up event handler
  let event = EventHandler::default();

  // connect to the FunkyChat server
  connect("127.0.0.1:13337", &event)?;

  loop {
    // draw the UI frame onto the terminal
    terminal.draw(|f| draw_app(f, &mut app))?;

    // wait for incoming events
    let event = event.next()?;
    match event {
      events::Event::UserInput(key_event) => match key_event.code {
        event::KeyCode::Backspace => {
          app.input.pop();
        }
        event::KeyCode::Char(c) => {
          app.input.push(c);
        }
        _ => {}
      }
      events::Event::ServerResponse(res) => match res {
        messages::response::Response::Welcome(welcome) => {
          println!("received welcome event");
          app.messages.push(Message {
            from: None,
            message: format!("Welcome, {}!", welcome.user_id).to_string()
          });
        }
        messages::response::Response::Echo(_) => {}
        messages::response::Response::Join(_) => {}
        messages::response::Response::Leave(_) => {}
        messages::response::Response::Chat(_) => {}
      }
    }
  }
}
