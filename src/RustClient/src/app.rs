pub struct App {
  /// Current value of the user's input.
  pub input: String,
  /// Received messages from the server.
  pub messages: Vec<String>,
}
impl Default for App {
  fn default() -> Self {
    App {
      input: String::new(),
      messages: Vec::new(),
    }
  }
}
