use tui::{
  text::Spans,
  widgets::{Block, Borders, Paragraph, Wrap},
};

use crate::app::App;

pub fn draw_messages(app: &mut App) -> Paragraph {
  let messages: Vec<Spans> = app
    .messages
    .iter()
    .enumerate()
    .map(|(_, msg)| {
      Spans::from(if let Some(from) = &msg.from {
        format!("[{}] {}: {}", msg.timestamp.format("%r"), from, msg.message)
      } else {
        format!("[{}] {}", msg.timestamp.format("%r"), msg.message)
      })
    })
    .rev()
    .collect();
  Paragraph::new(messages)
    .block(Block::default().title("Messages").borders(Borders::ALL))
    .wrap(Wrap { trim: true })
}
