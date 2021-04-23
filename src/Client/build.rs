use std::io::Error;

fn main() -> Result<(), Error> {
  // build the protobuf structs
  prost_build::compile_protos(&["../Protos/Messages.proto"], &["../Protos/"])?;
  Ok(())
}
