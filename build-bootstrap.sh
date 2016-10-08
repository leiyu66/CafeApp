#!/bin/bash
if test "$OS" = "Windows_NT"
then # For Windows
    .paket/paket.bootstrapper.exe
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
      exit $exit_code
    fi
else # For Non Windows
    mono .paket/paket.bootstrapper.exe
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
      exit $exit_code
    fi
fi
