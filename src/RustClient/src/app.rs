use chrono::{DateTime, Local};

pub struct Message {
  pub from: Option<String>,
  pub message: String,
  pub timestamp: DateTime<Local>
}

pub struct App {
  /// The local user's identifier.
  pub user_id: Option<String>,
  /// Current value of the user's input.
  pub input: String,
  /// Received messages from the server.
  pub messages: Vec<Message>,
}
impl Default for App {
  fn default() -> Self {
    App {
      user_id: None,
      input: String::new(),
      messages: Vec::new(),
    }
  }
}
