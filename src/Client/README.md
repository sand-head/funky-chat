# Client

This client is built using Rust 1.51.0.

## Running locally
1. [Install Rust](https://www.rust-lang.org/tools/install).
2. In this directory, run `cargo build`. This will automatically compile dependencies and build the Protobuf structs.
    * Alternatively, run `cargo build --release` to build for release, which removes debug features and optimizes the executable for release.
3. Afterwards, run `cargo run`, or run the `funky_chat-client` executable found in the `target/debug` directory.
    * If you build for release, the executable will be in the `target/release` directory.

## Resources used
* https://docs.rs/tui/0.14.0/tui/index.html
* https://docs.rs/prost/0.7.0/prost/trait.Message.html
* https://docs.rs/prost-build/0.7.0/prost_build/
* https://doc.rust-lang.org/book/ch16-02-message-passing.html
* https://doc.rust-lang.org/std/net/struct.TcpStream.html
* https://docs.rs/chrono/0.4.19/chrono/