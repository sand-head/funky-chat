use tui::{
  backend::Backend,
  layout::{Constraint, Direction, Layout},
  Frame,
};
use unicode_width::UnicodeWidthStr;

use crate::app::App;

mod chat_input;
mod messages;
mod online;

/// Draws the next frame of the application.
pub fn draw_app<B: Backend>(frame: &mut Frame<B>, app: &mut App) {
  // split up the terminal space into chunks
  let vert_chunks = Layout::default()
    .direction(Direction::Vertical)
    .margin(1)
    .constraints([Constraint::Min(1), Constraint::Length(3)].as_ref())
    .split(frame.size());
  let hori_chunks = Layout::default()
    .direction(Direction::Horizontal)
    .constraints([Constraint::Percentage(25), Constraint::Percentage(75)].as_ref())
    .split(vert_chunks[0]);

  // render online list
  frame.render_widget(online::draw_online_list(app), hori_chunks[0]);

  // render message history
  frame.render_widget(messages::draw_messages(app), hori_chunks[1]);

  // render chat input and set cursor to text position
  frame.render_widget(chat_input::draw_chat_input(app), vert_chunks[1]);
  frame.set_cursor(vert_chunks[1].x + app.input.width() as u16 + 1, vert_chunks[1].y + 1);
}
