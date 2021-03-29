use app::App;
use crossterm::event;
use event::Event;
use ui::draw;

use std::io;

use anyhow::Result;
use tui::{Terminal, backend::CrosstermBackend};

mod app;
mod ui;

fn main() -> Result<()> {
  let stdout = io::stdout();
  let backend = CrosstermBackend::new(stdout);
  let mut terminal = Terminal::new(backend)?;
  terminal.clear()?;

  let mut app = App::default();

  loop {
    terminal.draw(|f| draw(f, &mut app))?;

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
