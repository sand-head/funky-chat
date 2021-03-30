use tui::{text::Span, widgets::{Block, Borders, List, ListItem}};

use crate::app::App;

pub fn draw_online_list(app: &mut App) -> List {
  let online: Vec<ListItem> = app
    .online_users
    .iter()
    .enumerate()
    .map(|(_, user_id)| {
      let content = Span::raw(user_id);
      ListItem::new(content)
    })
    .collect();
  List::new(online).block(Block::default().title("Online").borders(Borders::ALL))
}
