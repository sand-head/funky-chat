use tui::{text::Span, widgets::{Block, Borders, List, ListItem}};

use crate::app::App;

pub fn draw_messages(app: &mut App) -> List {
  let messages: Vec<ListItem> = app
    .messages
    .iter()
    .enumerate()
    .map(|(_, msg)| {
      let content = Span::raw(if let Some(from) = &msg.from {
        format!("[{}] {}: {}", msg.timestamp.format("%r"), from, msg.message)
      } else {
        format!("[{}] {}", msg.timestamp.format("%r"), msg.message)
      });
      ListItem::new(content)
    })
    .collect();
  List::new(messages).block(Block::default().title("Messages").borders(Borders::ALL))
}
