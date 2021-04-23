use app::{App, AppState, Message, states::{StateHandler, connected::ConnectedState, server_input::ServerInputState}};
use events::EventHandler;

use std::io;

use anyhow::Result;
use tui::{
  backend::{Backend, CrosstermBackend},
  Terminal,
};

mod app;
mod connection;
mod events;
mod ui;

pub mod messages {
  include!(concat!(env!("OUT_DIR"), "/funkychat.protos.rs"));
}

fn handle_app_frame<B: Backend>(
  terminal: &mut Terminal<B>,
  event: &EventHandler,
  app: &mut App,
) -> Result<()> {
  match app.state {
    app::AppState::ServerInput => ServerInputState::handle(terminal, event, app),
    app::AppState::Connected => ConnectedState::handle(terminal, event, app),
    _ => panic!("wow we do not support that state yet! how'd you get here??"),
  }
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

  // show the UI for the current state
  while app.state != AppState::Disconnecting {
    handle_app_frame(&mut terminal, &event, &mut app)?;
  }

  terminal.clear()?;
  Ok(())
}
