use anyhow::Result;
use crossterm::event::KeyCode;
use tui::{backend::Backend, Terminal};

use crate::{app::{App, AppState}, events::EventHandler, ui::draw_server_input};

use super::StateHandler;

pub struct ServerInputState {}
impl StateHandler for ServerInputState {
  fn handle<B>(
    terminal: &mut Terminal<B>,
    event: &EventHandler,
    mut app: &mut App,
  ) -> Result<()>
  where
    B: Backend,
  {
    // draw the server input frame
    terminal.draw(|f| draw_server_input(f, app))?;

    // wait for incoming events
    let next_event = event.next()?;
    match next_event {
      crate::events::Event::UserInput(key) => {
        match key.code {
          KeyCode::Char(c) => {
            app.server.push(c);
          }
          KeyCode::Backspace => {
            app.server.pop();
          }
          KeyCode::Enter if app.server.len() > 0 => {
            // todo: show message instead of just obliterating the app on failure
            app.connect(&event)?;
            // todo: handle a possible "connecting" state
            // also we should probably not just set the state like this
            app.state = AppState::Connected;
          }
          _ => {}
        }
      }
      _ => {}
    }

    Ok(())
  }
}
