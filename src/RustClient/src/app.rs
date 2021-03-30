use chrono::{DateTime, Local};

pub struct Message {
  pub from: Option<String>,
  pub message: String,
  pub timestamp: DateTime<Local>,
}

/// Holds the current state of the application.
pub struct App {
  /// The local user's identifier.
  pub user_id: Option<String>,
  /// The identifiers of all connected users.
  pub online_users: Vec<String>,
  /// Current value of the user's input.
  pub input: String,
  /// Received messages from the server.
  pub messages: Vec<Message>,
}
impl Default for App {
  fn default() -> Self {
    App {
      user_id: None,
      online_users: Vec::new(),
      input: String::new(),
      messages: Vec::new(),
    }
  }
}
impl App {
  pub fn add_message(&mut self, msg: &str) {
    self.messages.push(Message {
      from: None,
      message: msg.to_string(),
      timestamp: Local::now(),
    });
  }
}
