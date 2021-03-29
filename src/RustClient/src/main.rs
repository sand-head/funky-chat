use app::App;
use crossterm::event;
use input::Input;
use ui::draw_app;

use std::io;

use anyhow::Result;
use tui::{backend::CrosstermBackend, Terminal};

mod app;
mod input;
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

  // set up user input
  let input = Input::default();

  loop {
    // draw the UI frame onto the terminal
    terminal.draw(|f| draw_app(f, &mut app))?;

    // handle user input
    let user_input = input.next()?;
    match user_input.code {
      event::KeyCode::Backspace => {
        app.input.pop();
      }
      event::KeyCode::Char(c) => {
        app.input.push(c);
      }
      _ => {}
    }
  }
}
