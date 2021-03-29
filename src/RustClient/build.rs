use std::io::Error;

fn main() -> Result<(), Error> {
  prost_build::compile_protos(&["../protos/messages.proto"], &["../protos/"])?;
  Ok(())
}
