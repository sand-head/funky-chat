use tui::widgets::{Block, Borders, Paragraph};

use crate::app::App;

pub fn draw_chat_input(app: &mut App) -> Paragraph {
  Paragraph::new(app.input.as_ref()).block(Block::default().title("Input").borders(Borders::ALL))
}
