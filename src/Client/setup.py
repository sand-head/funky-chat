from pathlib import Path
import os
import pkg_resources
from setuptools import Command, setup, find_packages
from subprocess import check_call

class BuildProtosCommand(Command):
    """Generates Python classes from shared Protobuf files"""
    user_options = []

    def initialize_options(self):
        pass

    def finalize_options(self):
        pass

    def run(self):
        from grpc_tools import protoc

        # get all the protobuf files
        protos = [str(proto) for proto in Path().glob("../Protos/**/*.proto")]

        # use protoc to compile them
        for proto in protos:
            command = [
                "-I=.",
                "--proto_path=../Protos",
                "--python_out=."
            ]
            if protoc.main(command + protos) != 0:
                raise Exception("Error: build proto failed")

setup(
    name = "funky-chat-client",
    version = "0.1.0",
    cmdclass = {
        "build_protos": BuildProtosCommand
    },
    packages = find_packages(where="src"),
    setup_requires = [
        # add any setup.py requirements here
        "grpcio-tools"
    ],
    install_requires = [
        # add any runtime requirements here
        "protobuf"
    ]
)