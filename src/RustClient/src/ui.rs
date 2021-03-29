use tui::{Frame, backend::Backend, layout::{Constraint, Direction, Layout}, widgets::{Block, Borders, List, Paragraph}};
use unicode_width::UnicodeWidthStr;

use crate::app::App;

pub fn draw<B: Backend>(frame: &mut Frame<B>, app: &mut App) {
  let chunks = Layout::default()
    .direction(Direction::Vertical)
    .margin(1)
    .constraints(
      [
        Constraint::Min(1),
        Constraint::Length(3)
      ].as_ref()
    )
    .split(frame.size());

  let messages = List::new(Vec::new())
    .block(
      Block::default()
        .title("Messages")
        .borders(Borders::ALL)
    );
  frame.render_widget(messages, chunks[0]);

  let input = Paragraph::new(app.input.as_ref())
    .block(
      Block::default()
        .title("Input")
        .borders(Borders::ALL)
    );
  frame.render_widget(input, chunks[1]);
  frame.set_cursor(
    chunks[1].x + app.input.width() as u16 + 1,
    chunks[1].y + 1
  );
}
