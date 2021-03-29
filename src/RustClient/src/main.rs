use app::App;
use crossterm::event;
use event::Event;
use ui::draw_app;

use std::io;

use anyhow::Result;
use tui::{Terminal, backend::CrosstermBackend};

mod app;
mod ui;

pub mod messages {
  include!(concat!(env!("OUT_DIR"), "/funkychat.protos.rs"));
}

fn main() -> Result<()> {
  let stdout = io::stdout();
  let backend = CrosstermBackend::new(stdout);
  let mut terminal = Terminal::new(backend)?;
  terminal.clear()?;

  let mut app = App::default();

  loop {
    terminal.draw(|f| draw_app(f, &mut app))?;

    // todo: handle user input
    let event = event::read()?;
    match event {
      Event::Key(_) => {
        println!("this is a key");
      }
      Event::Mouse(_) => {}
      Event::Resize(_, _) => {}
    }
    println!();
  }
}
